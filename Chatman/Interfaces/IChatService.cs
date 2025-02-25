using Chatman.Models;
using Chatman.Models.DTOs;

namespace Chatman.Interfaces
{
    public interface IChatService
    {
        #region //Get
        Task<ServiceResponse<List<MessageResponse>>> GetChatHistoryAsync(int userId, int friendId, int pageSize, int pageNumber);
        Task<ServiceResponse<int>> GetUnreadMessagesCountAsync(int userId);
        #endregion

        #region //Add
        Task<int> SaveMessageAsync(ChatMessage message);
        #endregion

        #region //Update
        Task<ServiceResponse<bool>> UpdateMessagesAsReadAsync(int senderId, int receiverId);
        #endregion

        #region //Delete

        #endregion
    }
}
