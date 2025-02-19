using Chatman.Models;
using Chatman.Models.DTOs;

namespace Chatman.Interfaces
{
    public interface IUserRepository
    {
        #region //Get
        Task<UserInfo> GetUserByEmailAsync(string email);
        Task<UserInfo> GetUserByIdAsync(int userId);
        Task<List<UserInfo>> GetUserByKeywordAsync(string keyword);
        Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId);
        Task<bool> CheckFriendStatusAsync(int userId, int friendId);
        Task<bool> CheckFriendRequestAsync(int userId, int friendId);
        #endregion

        #region //Add

        #endregion

        #region //Update
        Task<bool> UpdateUserAsync(UserInfo user);
        Task<bool> UpdateUserBio(UserInfo user);
        #endregion

        #region //Delete

        #endregion

        #region //Login
        Task<int> RegisterAsync(UserInfo user);
        Task<bool> IsEmailExistsAsync(string email);
        #endregion
    }
}
