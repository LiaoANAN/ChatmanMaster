using Azure.Core;
using Chatman.Data;
using Chatman.Interfaces;
using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace Chatman.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IDatabaseConnection _db;

        public ChatService(IChatRepository chatRepository, IUserService userService, IConfiguration configuration, ILogger<UserService> logger, IHubContext<ChatHub> hubContext, IDatabaseConnection db)
        {
            _chatRepository = chatRepository;
            _userService = userService;
            _configuration = configuration;
            _hubContext = hubContext;
            _logger = logger;
            _db = db;
        }

        #region //Get
        public async Task<ServiceResponse<List<MessageResponse>>> GetChatHistoryAsync(int userId, int friendId, int pageSize, int pageNumber)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    // 取得聊天記錄
                    var chatHistory = await _chatRepository.GetChatHistoryAsync(userId, friendId, pageSize, pageNumber, sqlConnection);

                    if (chatHistory == null || chatHistory.Count == 0)
                    {
                        return ServiceResponse<List<MessageResponse>>.ExcuteSuccess(new List<MessageResponse>(), "沒有聊天記錄");
                    }

                    // 將未讀訊息標記為已讀
                    if (!await _chatRepository.UpdateMessagesAsReadAsync(friendId, userId, sqlConnection))
                    {
                        ServiceResponse<List<MessageResponse>>.ExcuteError("將訊息已讀時錯誤!");
                    }

                    // 取得發送者資訊
                    var senderIds = chatHistory.Select(m => m.SenderId).Distinct().ToList();
                    var senderInfo = new Dictionary<int, UserInfo>();

                    foreach (var senderId in senderIds)
                    {
                        var user = await _userService.GetUserByIdAsync(senderId);
                        if (user != null)
                        {
                            senderInfo[senderId] = user;
                        }
                    }

                    // 轉換為 MessageResponse
                    var messageResponses = chatHistory.Select(m => new MessageResponse
                    {
                        MessageId = m.MessageId,
                        SenderId = m.SenderId,
                        SenderName = senderInfo.ContainsKey(m.SenderId) ? senderInfo[m.SenderId].UserName : "未知用戶",
                        SenderAvatar = senderInfo.ContainsKey(m.SenderId) ? senderInfo[m.SenderId].UserImage : "",
                        Content = m.Content,
                        MessageType = m.MessageType,
                        CreateDate = m.CreateDate,
                        IsRead = m.IsRead
                    }).ToList();

                    return ServiceResponse<List<MessageResponse>>.ExcuteSuccess(messageResponses);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取聊天記錄時發生錯誤: {UserId}, {FriendId}", userId, friendId);
                return ServiceResponse<List<MessageResponse>>.ServerError();
            }
        }

        public async Task<ServiceResponse<int>> GetUnreadMessagesCountAsync(int userId)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    var count = await _chatRepository.GetUnreadMessagesCountAsync(userId, sqlConnection);
                    return ServiceResponse<int>.ExcuteSuccess(count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤: {UserId}", userId);
                return ServiceResponse<int>.ServerError();
            }
        }
        #endregion

        #region //Add
        public async Task<int> SaveMessageAsync(ChatMessage message)
        {
            try
            {
                using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                {
                    using (SqlConnection sqlConnection = _db.CreateConnection())
                    {
                        var result = await _chatRepository.SaveMessageAsync(message, sqlConnection);
                        transactionScope.Complete();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        #region //Update
        public async Task<ServiceResponse<bool>> UpdateMessagesAsReadAsync(int senderId, int receiverId)
        {
            try
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection sqlConnection = _db.CreateConnection())
                    {
                        var result = await _chatRepository.UpdateMessagesAsReadAsync(senderId, receiverId, sqlConnection);
                        transactionScope.Complete();
                        return ServiceResponse<bool>.ExcuteSuccess(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "將訊息標記為已讀時發生錯誤: {SenderId}, {ReceiverId}", senderId, receiverId);
                return ServiceResponse<bool>.ServerError();
            }
        }
        #endregion

        #region //Delete

        #endregion
    }
}
