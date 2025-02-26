using Chatman.Filters;
using Chatman.Helpers;
using Chatman.Interfaces;
using Chatman.Models;
using Chatman.Models.DTOs;
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
