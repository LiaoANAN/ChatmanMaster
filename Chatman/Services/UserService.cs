using Chatman.Interfaces;
using Chatman.Models.DTOs;
using Chatman.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Azure.Core;
using Chatman.Helpers;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.AspNetCore.SignalR;
using Chatman.Data;
using System.Transactions;

namespace Chatman.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IDatabaseConnection _db;

        public UserService (IUserRepository userRepository, IConfiguration configuration, ILogger<UserService> logger, IHubContext<ChatHub> hubContext, IDatabaseConnection db)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _hubContext = hubContext;
            _logger = logger;
            _db = db;
        }

        #region //Login
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    if (request.Email == null)
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = "信箱不能為空"
                        };
                    }

                    if (request.Password == null)
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = "密碼不能為空"
                        };
                    }

                    var user = await _userRepository.GetUserByEmailAsync(request.Email, sqlConnection);

                    if (user == null)
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = "使用者不存在"
                        };
                    }

                    if (user.Status != "A")
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = "帳號已被停用"
                        };
                    }

                    if (!ValidatePasswordAsync(request.Password, user.Password))
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = "密碼錯誤"
                        };
                    }

                    // 生成 JWT Token
                    var token = GenerateJwtToken(user);

                    return new LoginResponse
                    {
                        Success = true,
                        Message = "登入成功",
                        Token = token,
                        UserInfo = user
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    Message = "登入過程發生錯誤"
                };
            }
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.UserId.ToString()),
                new Claim("userName", user.UserName),
                new Claim("email", user.Email),
                new Claim("status", user.Status),
                // 如果需要，可以添加更多 claims
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidatePasswordAsync(string inputPassword, string hashedPassword)
        {
            var hashedInput = WebHelper.HashPassword(inputPassword);
            return hashedPassword == hashedInput;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                {
                    using (SqlConnection sqlConnection = _db.CreateConnection())
                    {
                        //確認信箱是否重複
                        if (await IsEmailExistsAsync(request.Email))
                        {
                            return RegisterResponse.ErrorResponse("此電子郵件已被註冊");
                        }

                        var (userId, errorMessage) = await _userRepository.RegisterAsync(new UserInfo
                        {
                            UserName = request.Username,
                            Email = request.Email,
                            Password = WebHelper.HashPassword(request.Password),
                            Gender = request.Gender,
                            Birthday = request.Birthday,
                            Status = "A",
                            CreateDate = DateTime.Now,
                            CreateUserId = 0
                        }, sqlConnection);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            return RegisterResponse.ErrorResponse(errorMessage);
                        }

                        transactionScope.Complete();

                        if (userId > 0)
                        {
                            return new RegisterResponse()
                            {
                                Success = true,
                                Message = "註冊成功",
                                UserId = userId
                            };
                        }

                        return RegisterResponse.ErrorResponse("註冊失敗!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return RegisterResponse.ErrorResponse(ex.Message);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            using (SqlConnection sqlConnection = _db.CreateConnection())
            {
                return await _userRepository.IsEmailExistsAsync(email, sqlConnection);
            }
        }
        #endregion

        #region //Get
        public async Task<UserInfo> GetUserByEmailAsync(string email)
        {
            using (SqlConnection sqlConnection = _db.CreateConnection())
            {
                return await _userRepository.GetUserByEmailAsync(email, sqlConnection);
            }
        }

        public async Task<UserInfo> GetUserByIdAsync(int userId)
        {
            using (SqlConnection sqlConnection = _db.CreateConnection())
            {
                return await _userRepository.GetUserByIdAsync(userId, sqlConnection);
            }
        }

        public async Task<List<GetUserByKeywordResponse>> GetUserByKeywordAsync(string keyword, int userId)
        {
            using (SqlConnection sqlConnection = _db.CreateConnection())
            {
                var users = await _userRepository.GetUserByKeywordAsync(keyword, sqlConnection);

                List<GetUserByKeywordResponse> responses = new List<GetUserByKeywordResponse>();
                foreach (var user in users)
                {
                    if (user.UserId == userId) continue;

                    bool isFriend = await _userRepository.CheckFriendStatusAsync(userId, user.UserId, sqlConnection);
                    bool isRequest = await _userRepository.CheckFriendRequestAsync(userId, user.UserId, sqlConnection);

                    responses.Add(new GetUserByKeywordResponse()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Email = user.Email,
                        Gender = user.Gender,
                        Bio = user.Bio,
                        UserImage = user.UserImage,
                        FriendStatus = isFriend ? "Y" : isRequest ? "P" : "N"
                    });
                }

                return responses;
            }
        }

        public async Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId)
        {
            using (SqlConnection sqlConnection = _db.CreateConnection())
            {
                return await _userRepository.GetFriendsByUserIdAsync(userId, sqlConnection);
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(int userId)
        {
            using (SqlConnection sqlConnection = _db.CreateConnection())
            {
                return await _userRepository.GetUnreadNotificationsAsync(userId, sqlConnection);
            }
        }
        #endregion

        #region //Add
        public async Task<ServiceResponse<bool>> AddFriendRequestAsync(AddFriendRequestRequest request)
        {
            using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    #region //確認是否已是好友狀態
                    if (await _userRepository.CheckFriendStatusAsync(request.SendId, request.ReceiverId, sqlConnection))
                    {
                        return ServiceResponse<bool>.ExcuteError("與此用戶已是好友狀態!");
                    }
                    #endregion

                    #region //確認是否已經有申請過，且尚未回應
                    if (await _userRepository.CheckFriendRequestAsync(request.SendId, request.ReceiverId, sqlConnection))
                    {
                        return ServiceResponse<bool>.ExcuteError("已經申請且用戶尚未回應!");
                    }
                    #endregion

                    #region //新增好友申請
                    int? friendRequestId = await _userRepository.AddFriendRequestAsync(new FriendRequest()
                    {
                        SenderId = request.SendId,
                        ReceiverId = request.ReceiverId,
                        Message = request.Message,
                        Status = "P",
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateUserId = request.SendId,
                        UpdateUserId = request.SendId
                    }, sqlConnection);

                    if (friendRequestId == null)
                    {
                        return ServiceResponse<bool>.ExcuteError("新增好友申請資料發生錯誤!");
                    }
                    #endregion

                    #region //新增通知資料
                    int? notificationId = await _userRepository.AddNotificationAsync(new Notification()
                    {
                        UserId = request.ReceiverId,
                        Type = "friendRequest",
                        Message = request.Message,
                        RequestId = friendRequestId,
                        SenderId = request.SendId,
                        IsRead = false,
                        Status = "A",
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateUserId = request.SendId,
                        UpdateUserId = request.SendId
                    }, sqlConnection);

                    if (notificationId == null)
                    {
                        return ServiceResponse<bool>.ExcuteError("新增訊息資料發生錯誤!");
                    }
                    #endregion

                    // 通過 SignalR 發送通知
                    try
                    {
                        await _hubContext.Clients.Group(request.ReceiverId.ToString())
                            .SendAsync("ReceiveFriendRequest", new NotificationResponse()
                            {
                                NotificationId = (int)notificationId,
                                RequestId = (int)friendRequestId,
                                SenderId = request.SendId,
                                SenderName = request.SenderName,
                                SenderImage = request.SenderImage,
                                Message = request.Message,
                                CreateDate = DateTime.Now
                            });

                        _logger.LogInformation($"Friend request notification sent to user {request.ReceiverId}");
                    }
                    catch (Exception ex)
                    {
                        return ServiceResponse<bool>.ExcuteError(ex.Message);
                    }
                }
                transactionScope.Complete();
            }

            return ServiceResponse<bool>.ExcuteSuccess();
        }
        #endregion

        #region //Update
        public async Task<bool> UpdateUserBioAsync(UserInfo user)
        {
            using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    var result = await _userRepository.UpdateUserBioAsync(user, sqlConnection);
                    transactionScope.Complete();
                    return result;
                }
            }
        }

        public async Task<ServiceResponse<bool>> UpdateFriendRequestAsync(FriendRequest request)
        {
            using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    FriendRequest _friendRequest = await _userRepository.GetFriendRequestByIdAsync(request.FriendRequestId, sqlConnection);
                    if (_friendRequest == null)
                    {
                        return ServiceResponse<bool>.ExcuteError("好友申請需求資料錯誤!");
                    }

                    if (await _userRepository.IsFriendRequestAsync(_friendRequest.FriendRequestId, sqlConnection))
                    {
                        return ServiceResponse<bool>.ExcuteError("此好友申請需求已處理!");
                    }

                    if (await _userRepository.CheckFriendStatusAsync(_friendRequest.SenderId, _friendRequest.ReceiverId, sqlConnection))
                    {
                        return ServiceResponse<bool>.ExcuteError("與此用戶已是好友關係!");
                    }

                    if (!await _userRepository.UpdateFriendRequestAsync(request, sqlConnection))
                    {
                        return ServiceResponse<bool>.ExcuteError("處理好友申請需求時錯誤!");
                    }

                    if (request.Status == "A")
                    {
                        #region //新增好友關係名單
                        FriendRelation friendRelation = new FriendRelation()
                        {
                            UserId = _friendRequest.SenderId,
                            FriendId = _friendRequest.ReceiverId,
                            Status = "A",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            CreateUserId = _friendRequest.SenderId,
                            UpdateUserId = _friendRequest.SenderId
                        };

                        if (await _userRepository.AddFriendRelationAsync(friendRelation, sqlConnection) == null)
                        {
                            return ServiceResponse<bool>.ExcuteError("新增好友時錯誤!");
                        }

                        friendRelation.UserId = _friendRequest.ReceiverId;
                        friendRelation.FriendId = _friendRequest.SenderId;
                        if (await _userRepository.AddFriendRelationAsync(friendRelation, sqlConnection) == null)
                        {
                            return ServiceResponse<bool>.ExcuteError("新增好友時錯誤!");
                        }
                        #endregion

                        //更新雙方好友列表
                        await _hubContext.Clients.Group(friendRelation.UserId.ToString())
                            .SendAsync("UpdateFriendList");

                        await _hubContext.Clients.Group(friendRelation.FriendId.ToString())
                            .SendAsync("UpdateFriendList");
                    }
                }
                transactionScope.Complete();
            }
            return ServiceResponse<bool>.ExcuteSuccess();
        }

        public async Task<ServiceResponse<bool>> UpdateNotificationStatusAsync(Notification notification)
        {
            using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    if (!await _userRepository.UpdateNotificationStatusAsync(notification, sqlConnection))
                    {
                        return ServiceResponse<bool>.ExcuteError("更改通知訊息時錯誤!");
                    }
                }
                transactionScope.Complete();
            }
            return ServiceResponse<bool>.ExcuteSuccess();
        }
        #endregion

        #region //Delete

        #endregion
    }
}
