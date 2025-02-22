using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Chatman.Interfaces
{
    public interface IUserRepository
    {
        #region //Get
        Task<UserInfo> GetUserByEmailAsync(string email, SqlConnection sqlConnection);
        Task<UserInfo> GetUserByIdAsync(int userId, SqlConnection sqlConnection);
        Task<List<UserInfo>> GetUserByKeywordAsync(string keyword, SqlConnection sqlConnection);
        Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId, SqlConnection sqlConnection);
        Task<bool> CheckFriendStatusAsync(int userId, int friendId, SqlConnection sqlConnection);
        Task<bool> CheckFriendRequestAsync(int userId, int friendId, SqlConnection sqlConnection);
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId, SqlConnection sqlConnection);
        Task<FriendRequest> GetFriendRequestByIdAsync(int friendRequestId, SqlConnection sqlConnection);
        Task<bool> IsFriendRequestAsync(int friendRequestId, SqlConnection sqlConnection);
        #endregion

        #region //Add
        Task<int?> AddFriendRequestAsync(FriendRequest request, SqlConnection sqlConnection);
        Task<int?> AddNotificationAsync(Notification notification, SqlConnection sqlConnection);
        Task<int?> AddFriendRelationAsync(FriendRelation friendRelation, SqlConnection sqlConnection);
        #endregion

        #region //Update
        Task<bool> UpdateUserAsync(UserInfo user, SqlConnection sqlConnection);
        Task<bool> UpdateUserBioAsync(UserInfo user, SqlConnection sqlConnection);
        Task<bool> UpdateFriendRequestAsync(FriendRequest request, SqlConnection sqlConnection);
        Task<bool> UpdateNotificationStatusAsync(Notification notification, SqlConnection sqlConnection);
        #endregion

        #region //Delete

        #endregion

        #region //Login
        Task<(int, string)> RegisterAsync(UserInfo user, SqlConnection sqlConnection);
        Task<bool> IsEmailExistsAsync(string email, SqlConnection sqlConnection);
        #endregion
    }
}
