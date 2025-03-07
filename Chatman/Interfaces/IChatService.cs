﻿using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Chatman.Interfaces
{
    public interface IChatService
    {
        #region //Get
        Task<ServiceResponse<List<MessageResponse>>> GetChatHistoryAsync(int userId, int friendId, int pageSize, int pageNumber);
        Task<int> GetUnreadMessagesCountAsync(int userId);
        Task<int> GetUnreadMessagesCountFromFriendAsync(int userId, int friendId);
        Task<ServiceResponse<List<RecentChatsResponse>>> GetRecentChatsAsync(int userId);
        Task<ServiceResponse<List<RecentChatsResponse>>> GetRecentChatsByKeywordAsync(string keyword, int userId);
        Task<ServiceResponse<MessagePageResponse>> GetMessagePageAsync(int userId, int friendId, int messageId, int pageSize);
        Task<ServiceResponse<List<MessageResponse>>> GetNewerMessagesAsync(int userId, int friendId, int lastMessageId, int pageSize);
        #endregion

        #region //Add
        Task<int> SaveMessageAsync(ChatMessage message);
        #endregion

        #region //Update
        Task<ServiceResponse<bool>> UpdateMessagesAsReadAsync(int senderId, int receiverId);
        Task<ServiceResponse<bool>> RetractMessageAsync(RetractMessageRequest request);
        #endregion

        #region //Delete

        #endregion
    }
}
