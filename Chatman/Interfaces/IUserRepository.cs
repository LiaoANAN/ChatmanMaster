using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Chatman.Interfaces
{
    public interface IUserRepository
    {
        #region //Get
        Task<UserInfo> GetUserByEmailAsync(string email);
        Task<UserInfo> GetUserByIdAsync(int userId);
        Task<List<UserInfo>> GetUserByKeywordAsync(string keyword, SqlConnection sqlConnection);
        Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId);
        Task<bool> CheckFriendStatusAsync(int userId, int friendId, SqlConnection sqlConnection);
        Task<bool> CheckFriendRequestAsync(int userId, int friendId, SqlConnection sqlConnection);
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId);
        #endregion

        #region //Add
        Task<int> AddFriendRequestAsync(FriendRequest request, SqlConnection sqlConnection);
        Task<(int, string)> AddNotificationAsync(Notification notification, SqlConnection sqlConnection);
        #endregion

        #region //Update
        Task<bool> UpdateUserAsync(UserInfo user);
        Task<bool> UpdateUserBioAsync(UserInfo user);
        #endregion

        #region //Delete

        #endregion

        #region //Login
        Task<int> RegisterAsync(UserInfo user);
        Task<bool> IsEmailExistsAsync(string email);
        #endregion
    }
}
