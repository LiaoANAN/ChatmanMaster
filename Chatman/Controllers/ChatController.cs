using Azure;
using Chatman.Filters;
using Chatman.Helpers;
using Chatman.Interfaces;
using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chatman.Controllers
{
    [Authentication]
    public class ChatController : WebController
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            IChatService chatService,
            ILogger<ChatController> logger,
            IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _logger = logger;
            _hubContext = hubContext;
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
        #endregion

        #region //Delete

        #endregion
    }
}
