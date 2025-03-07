﻿using Azure.Core;
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
                        FileName = m.FileName,
                        FileSize = m.FileSize,
                        MessageType = m.MessageType,
                        CreateDate = m.CreateDate,
                        IsRead = m.IsRead,
                        Status = m.Status,
                        ReplyTo = m.ReplyToMessageId.HasValue ? new ReplyInfo
                        {
                            MessageId = m.ReplyToMessageId.Value,
                            SenderName = m.ReplyToSenderName,
                            Content = m.ReplyToContent,
                            MessageType = m.ReplyToMessageType,
                            ImageUrl = m.ReplyToImageUrl,
                            FileName = m.ReplyToFileName
                        } : null
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

        public async Task<int> GetUnreadMessagesCountAsync(int userId)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    return await _chatRepository.GetUnreadMessagesCountAsync(userId, sqlConnection);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤: {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> GetUnreadMessagesCountFromFriendAsync(int userId, int friendId)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    return await _chatRepository.GetUnreadMessagesCountFromFriendAsync(userId, friendId, sqlConnection);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤: {UserId}", userId);
                return 0;
            }
        }

        public async Task<ServiceResponse<List<RecentChatsResponse>>> GetRecentChatsAsync(int userId)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    var recentChats = await _chatRepository.GetRecentChatsAsync(userId, sqlConnection);
                    return ServiceResponse<List<RecentChatsResponse>>.ExcuteSuccess(recentChats);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取最近聊天資料時異常: {UserId}", userId);
                return ServiceResponse<List<RecentChatsResponse>>.ServerError();
            }
        }

        public async Task<ServiceResponse<List<RecentChatsResponse>>> GetRecentChatsByKeywordAsync(string keyword, int userId)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    var recentChats = await _chatRepository.GetRecentChatsByKeywordAsync(keyword, userId, sqlConnection);
                    return ServiceResponse<List<RecentChatsResponse>>.ExcuteSuccess(recentChats);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取最近聊天資料時異常: {UserId}", userId);
                return ServiceResponse<List<RecentChatsResponse>>.ServerError();
            }
        }

        public async Task<ServiceResponse<MessagePageResponse>> GetMessagePageAsync(int userId, int friendId, int messageId, int pageSize)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    var messagePage = await _chatRepository.GetMessagePageAsync(userId, friendId, messageId, pageSize, sqlConnection);
                    return ServiceResponse<MessagePageResponse>.ExcuteSuccess(messagePage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取訊息頁數時錯誤: {messageId}", messageId);
                return ServiceResponse<MessagePageResponse>.ServerError();
            }
        }

        public async Task<ServiceResponse<List<MessageResponse>>> GetNewerMessagesAsync(int userId, int friendId, int lastMessageId, int pageSize)
        {
            try
            {
                using (SqlConnection sqlConnection = _db.CreateConnection())
                {
                    // 取得聊天記錄
                    var chatHistory = await _chatRepository.GetNewerMessagesAsync(userId, friendId, lastMessageId, pageSize, sqlConnection);

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

        public async Task<ServiceResponse<bool>> RetractMessageAsync(RetractMessageRequest request)
        {
            try
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    using (SqlConnection sqlConnection = _db.CreateConnection())
                    {
                        // 檢查權限 - 只有發送者可以收回自己的訊息
                        // 也可以添加時間限制，例如只能收回 5 分鐘內的訊息

                        var result = await _chatRepository.RetractMessageAsync(request.MessageId, request.SenderId, sqlConnection);

                        if (!result)
                        {
                            return ServiceResponse<bool>.ExcuteError("收回訊息失敗或無權限收回此訊息");
                        }

                        transactionScope.Complete();
                        return ServiceResponse<bool>.ExcuteSuccess(true, "訊息已收回");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "收回訊息失敗: {MessageId}", request.MessageId);
                return ServiceResponse<bool>.ServerError();
            }
        }
        #endregion

        #region //Delete

        #endregion
    }
}
