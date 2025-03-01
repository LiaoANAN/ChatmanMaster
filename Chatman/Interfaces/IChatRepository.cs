using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Chatman.Interfaces
{
    public interface IChatRepository
    {
        #region //Get
        Task<List<ChatMessage>> GetChatHistoryAsync(int userId, int friendId, int pageSize, int pageNumber, SqlConnection sqlConnection);
        Task<int> GetUnreadMessagesCountAsync(int userId, SqlConnection sqlConnection);
        Task<int> GetUnreadMessagesCountFromFriendAsync(int userId, int friendId, SqlConnection sqlConnection);
        Task<List<RecentChatsResponse>> GetRecentChatsAsync(int userId, SqlConnection sqlConnection);
        #endregion

        #region //Add
        Task<int> SaveMessageAsync(ChatMessage message, SqlConnection sqlConnection);
        #endregion

        #region //Update
        Task<bool> UpdateMessagesAsReadAsync(int senderId, int receiverId, SqlConnection sqlConnection);
        #endregion

        #region //Delete

        #endregion
    }
}
