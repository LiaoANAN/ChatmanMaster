using Chatman.Interfaces;
using Chatman.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Chatman.Helpers;
using Chatman.Models;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Chatman.Filters;
using Chatman.Services;

namespace Chatman.Controllers
{
    [Authentication]
    public class UserController : WebController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region //View
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        #endregion

        #region //Get
        [HttpGet]
        public async Task<IActionResult> GetFriendsList()
        {
            try
            {
                var userInfo = WebHelper.GetCurrentUser(this.HttpContext);
                if (userInfo == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入!"
                    });
                }

                var friends = await _userService.GetFriendsByUserIdAsync(userInfo.UserId);

                return Ok(new
                {
                    success = true,
                    data = friends
                });
            }
            catch (Exception ex) 
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("api/user/getUser")]
        public async Task<IActionResult> GetUserInfo(string keyword)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var users = await _userService.GetUserByKeywordAsync(keyword, user.UserId);

                return Ok(new
                {
                    success = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("api/user/getUnreadNotifications")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var result = await _userService.GetUnreadNotificationsAsync(user.UserId);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region //Add
        [HttpPost("api/user/sendFriendRequest")]
        public async Task<IActionResult> AddFriendRequest([FromBody] AddFriendRequestRequest request)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                request.SendId = user.UserId;
                request.SenderName = user.UserName;
                request.SenderImage = user.UserImage;

                var response = await _userService.AddFriendRequestAsync(request);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add friend request failed");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }
        #endregion

        #region //Update
        [HttpPost("api/user/updateUserBio")]
        public async Task<IActionResult> UpdateUserBio([FromBody] UpdateUserBioRequest request)
        {
            try
            {
                var userInfo = WebHelper.GetCurrentUser(this.HttpContext);
                if (userInfo == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                int userId = userInfo.UserId;
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "查無用戶資料"
                    });
                }

                user.Bio = request.Bio;
                user.UpdateDate = DateTime.Now;
                user.UpdateUserId = userId;

                var response = await _userService.UpdateUserBioAsync(user);

                if (response)
                {
                    return Ok(new
                    {
                        success = true
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "修改個人簽名失敗"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return StatusCode(500, new { message = "內部伺服器錯誤" });
            }
        }

        [HttpPost("api/user/handleFriendRequest")]
        public async Task<IActionResult> UpdateFriendRequest([FromBody] FriendRequest request)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                request.UpdateDate = DateTime.Now;
                request.UpdateUserId = user.UserId;
                var response = await _userService.UpdateFriendRequestAsync(request);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }

        [HttpPost("api/user/updateNotificationStatus")]
        public async Task<IActionResult> UpdateNotificationStatus([FromBody] Notification notification)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                notification.UpdateDate = DateTime.Now;
                notification.UpdateUserId = user.UserId;

                var response = await _userService.UpdateNotificationStatusAsync(notification);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }

        [HttpPost("api/user/updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                UserInfo userInfo = await _userService.GetUserByIdAsync(user.UserId);
                if (userInfo == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "查無用戶資料"
                    });
                }

                userInfo.UserName = request.UserName;
                userInfo.Gender = request.Gender;
                userInfo.Birthday = request.Birthday;
                userInfo.Bio = request.Bio;
                userInfo.UpdateDate = DateTime.Now;
                userInfo.UpdateUserId = userInfo.UserId;

                // 處理頭像上傳
                string uploadedFilePath = "";
                if (request.UserImage != null && request.UserImage.Length > 0)
                {
                    // 處理文件保存路徑
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "user_images");
                    Directory.CreateDirectory(uploadsFolder); // 確保目錄存在

                    // 生成唯一文件名
                    string uniqueFileName = $"{userInfo.UserId.ToString()}_{Guid.NewGuid()}{Path.GetExtension(request.UserImage.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    uploadedFilePath = filePath; // 記錄文件路徑，以便在失敗時刪除

                    // 保存文件
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.UserImage.CopyToAsync(fileStream);
                    }

                    // 設置用戶頭像路徑
                    userInfo.UserImage = $"/uploads/user_images/{uniqueFileName}";
                }

                var response = await _userService.UpdateProfileAsync(userInfo);

                if (response.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new { userImage = userInfo.UserImage }
                    });
                }
                else
                {
                    if (uploadedFilePath != null && System.IO.File.Exists(uploadedFilePath))
                    {
                        try
                        {
                            System.IO.File.Delete(uploadedFilePath);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, $"刪除未使用的頭像文件失敗: {userInfo.UserImage}");
                        }
                    }
                }

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update ProFile failed");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }

        [HttpPost("api/user/markAllNotificationsAsRead")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                var user = WebHelper.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "用戶未登入" });
                }

                var response = await _userService.UpdateAllMessagesAsReadAsync(user.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "將訊息標記為已讀時發生錯誤");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }
        #endregion

        #region //Delete

        #endregion

        #region //Login
        [HttpPost]
        public async Task<IActionResult> UserLogin([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _userService.LoginAsync(request);
                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, new { message = "內部伺服器錯誤" });
            }
        }
        #endregion

        #region //Register
        [HttpPost]
        public async Task<IActionResult> UserRegister([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _userService.RegisterAsync(request);
                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return StatusCode(500, new { message = "內部伺服器錯誤" });
            }
        }
        #endregion
    }
}
