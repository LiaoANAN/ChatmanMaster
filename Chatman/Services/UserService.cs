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

                var user = await _userRepository.GetUserByEmailAsync(request.Email);

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
                //確認信箱是否重複
                if (await IsEmailExistsAsync(request.Email))
                {
                    return RegisterResponse.ErrorResponse("此電子郵件已被註冊");
                }

                UserInfo user = new UserInfo
                {
                    UserName = request.Username,
                    Email = request.Email,
                    Password = WebHelper.HashPassword(request.Password),
                    Gender = request.Gender,
                    Birthday = request.Birthday,
                    Status = "A",
                    CreateDate = DateTime.Now,
                    CreateUserId = 0
                };

                int userId = await _userRepository.RegisterAsync(user);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return RegisterResponse.ErrorResponse(ex.Message);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userRepository.IsEmailExistsAsync(email);
        }
        #endregion

        #region //Get
        public async Task<UserInfo> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<UserInfo> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
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
            return await _userRepository.GetFriendsByUserIdAsync(userId);
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(int userId)
        {
            return await _userRepository.GetUnreadNotificationsAsync(userId);
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
                    var requestId = await _userRepository.AddFriendRequestAsync(new FriendRequest()
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
                    #endregion

                    #region //新增通知資料
                    var (notificationId, errorMessage) = await _userRepository.AddNotificationAsync(new Notification()
                    {
                        UserId = request.ReceiverId,
                        Type = "friendRequest",
                        Message = request.Message,
                        RequestId = requestId,
                        SenderId = request.SendId,
                        IsRead = false,
                        Status = "A",
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateUserId = request.SendId,
                        UpdateUserId = request.SendId
                    }, sqlConnection);

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        return ServiceResponse<bool>.ExcuteError(errorMessage);
                    }
                    #endregion

                    // 通過 SignalR 發送通知
                    try
                    {
                        await _hubContext.Clients.Group(request.ReceiverId.ToString())
                            .SendAsync("ReceiveFriendRequest", new NotificationResponse()
                            {
                                RequestId = notificationId,
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
            return await _userRepository.UpdateUserBioAsync(user);
        }
        #endregion

        #region //Delete

        #endregion
    }
}
