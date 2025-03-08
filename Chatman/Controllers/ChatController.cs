using Azure;
using Chatman.Filters;
using Chatman.Helpers;
using Chatman.Interfaces;
using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;

namespace Chatman.Controllers
{
    [Authentication]
    public class ChatController : WebController
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ChatController(
            IChatService chatService,
            ILogger<ChatController> logger,
            IHubContext<ChatHub> hubContext,
            IWebHostEnvironment hostingEnvironment)
        {
            _chatService = chatService;
            _logger = logger;
            _hubContext = hubContext;
            _hostingEnvironment = hostingEnvironment;
        }

        #region //View
        public IActionResult ChatRoom()
        {
            return View();
        }
        public IActionResult PrivateRoom()
        {
            return View();
        }
        #endregion

        #region //Get
        #region //取得特定好友歷史聊天訊息
        [HttpGet("api/chat/history")]
        public async Task<IActionResult> GetChatHistory(int friendId, int pageSize = 20, int pageNumber = 1)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "用戶未登入" });
                }

                var response = await _chatService.GetChatHistoryAsync(user.UserId, friendId, pageSize, pageNumber);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取聊天記錄時發生錯誤");
                return StatusCode(500, ServiceResponse<List<MessageResponse>>.ServerError());
            }
        }
        #endregion

        #region //取得未讀訊息總數
        [HttpGet("api/chat/unreadCount")]
        public async Task<IActionResult> GetUnReadCount()
        {
            try
            {
                UserInfo user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var count = await _chatService.GetUnreadMessagesCountAsync(user.UserId);

                return Ok(new
                {
                    success = true,
                    count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region //取得未讀訊息總數(From Friend)
        [HttpGet("api/chat/unreadCountFromFriend")]
        public async Task<IActionResult> GetUnReadCountFromFriend(int friendId)
        {
            try
            {
                UserInfo user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var count = await _chatService.GetUnreadMessagesCountFromFriendAsync(user.UserId, friendId);

                return Ok(new
                {
                    success = true,
                    count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region //取得近期聊天紀錄
        [HttpGet("api/chat/recentChats")]
        public async Task<IActionResult> GetRecentChats()
        {
            try
            {
                UserInfo user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var response = await _chatService.GetRecentChatsAsync(user.UserId);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得聊天紀錄列表發生錯誤!");
                return StatusCode(500, ServiceResponse<List<RecentChatsResponse>>.ServerError());
            }
        }
        #endregion

        #region //取得近期聊天紀錄 By Keyword
        [HttpGet("api/chat/recentChatsByKeyword")]
        public async Task<IActionResult> GetRecentChatsByKeyword(string keyword)
        {
            try
            {
                UserInfo user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var response = await _chatService.GetRecentChatsByKeywordAsync(keyword, user.UserId);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得聊天紀錄列表發生錯誤!");
                return StatusCode(500, ServiceResponse<List<RecentChatsResponse>>.ServerError());
            }
        }
        #endregion

        #region //取得訊息頁數
        [HttpGet("api/chat/findMessagePage")]
        public async Task<IActionResult> GetMessagePage(int friendId, int messageId, int pageSize)
        {
            try
            {
                UserInfo user = WebHelper.GetCurrentUser(this.HttpContext);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用戶未登入"
                    });
                }

                var response = await _chatService.GetMessagePageAsync(user.UserId, friendId, messageId, pageSize);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得訊息頁數發生錯誤!");
                return StatusCode(500, ServiceResponse<MessagePageResponse>.ServerError());
            }
        }
        #endregion

        #region //取得特定好友歷史聊天訊息
        [HttpGet("api/chat/newer")]
        public async Task<IActionResult> GetNewerMessages(int friendId, int lastMessageId, int pageSize = 20)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "用戶未登入" });
                }

                var response = await _chatService.GetNewerMessagesAsync(user.UserId, friendId, lastMessageId, pageSize);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取聊天記錄時發生錯誤");
                return StatusCode(500, ServiceResponse<List<MessageResponse>>.ServerError());
            }
        }
        #endregion
        #endregion

        #region //Add
        #endregion

        #region //Update
        [HttpPost("api/chat/markAsRead")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] ChatMessage message)
        {
            try
            {
                var user = WebHelper.GetCurrentUser(HttpContext);
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "用戶未登入" });
                }

                var response = await _chatService.UpdateMessagesAsReadAsync(message.SenderId, user.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "將訊息標記為已讀時發生錯誤");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }

        [HttpPost("api/chat/retractMessage")]
        public async Task<IActionResult> RetractMessage([FromBody] RetractMessageRequest request)
        {
            try
            {
                var userInfo = WebHelper.GetCurrentUser(this.HttpContext);
                if (userInfo == null || userInfo.UserId != request.SenderId)
                {
                    return Unauthorized(new { success = false, message = "未授權的操作" });
                }

                var response = await _chatService.RetractMessageAsync(request);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "收回訊息失敗");
                return StatusCode(500, ServiceResponse<bool>.ServerError());
            }
        }
        #endregion

        #region //Delete

        #endregion

        #region //File
        #region //上傳圖片
        [HttpPost("api/chat/uploadImage")]
        public async Task<IActionResult> UploadImage()
        {
            try
            {
                var userInfo = WebHelper.GetCurrentUser(HttpContext);
                if (userInfo == null)
                {
                    return BadRequest(new { success = false, message = "用戶未登入!" });
                }

                var receiverId = HttpContext.Request.Form["receiverId"];
                if (string.IsNullOrEmpty(receiverId))
                {
                    return BadRequest(new { success = false, message = "缺少接收者ID" });
                }

                var file = Request.Form.Files[0];
                if (file == null)
                {
                    return BadRequest(new { success = false, message = "缺少文件" });
                }

                // 檢查文件是否為圖片
                if (!file.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { success = false, message = "僅支援圖片格式" });
                }

                // 檢查文件大小 (5MB上限)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "圖片大小不能超過5MB" });
                }

                // 創建儲存路徑
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "chat_images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 生成唯一文件名
                string uniqueFileName = $"{Guid.NewGuid()}_{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 保存文件
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // 返回文件URL
                string fileUrl = $"/uploads/chat_images/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    message = "圖片上傳成功",
                    data = new
                    {
                        url = fileUrl,
                        fileName = uniqueFileName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳圖片失敗");
                return StatusCode(500, new { success = false, message = "上傳圖片時發生錯誤" });
            }
        }
        #endregion

        #region //上傳檔案
        [HttpPost("api/chat/uploadFile")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var userInfo = WebHelper.GetCurrentUser(HttpContext);
                if (userInfo == null)
                {
                    return BadRequest(new { success = false, message = "用戶未登入!" });
                }

                var receiverId = HttpContext.Request.Form["receiverId"];
                if (string.IsNullOrEmpty(receiverId))
                {
                    return BadRequest(new { success = false, message = "缺少接收者ID" });
                }

                var file = Request.Form.Files[0];
                if (file == null)
                {
                    return BadRequest(new { success = false, message = "缺少文件" });
                }

                // 檢查文件是否為圖片
                var fileType = HttpContext.Request.Form["fileType"];
                if (fileType == "image" && !file.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { success = false, message = "僅支援圖片格式" });
                }

                // 檢查文件大小 (5MB上限)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "圖片大小不能超過5MB" });
                }

                // 創建儲存路徑
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "chat_files");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 生成唯一文件名
                string uniqueFileName = $"{Guid.NewGuid()}_{ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"')}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 保存文件
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // 返回文件URL
                string fileUrl = $"/uploads/chat_files/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    message = "檔案上傳成功",
                    data = new
                    {
                        url = fileUrl,
                        fileName = uniqueFileName,
                        fileType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳檔案失敗");
                return StatusCode(500, new { success = false, message = "上傳檔案時發生錯誤" });
            }
        }
        #endregion
        #endregion
    }
}
