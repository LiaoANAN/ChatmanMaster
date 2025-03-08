using Chatman.Models.DTOs;
using Chatman.Models;

namespace Chatman.Interfaces
{
    public interface IUserService
    {
        #region //Login
        Task<LoginResponse> LoginAsync(LoginRequest request);
        bool ValidatePasswordAsync(string hashedPassword, string inputPassword);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<bool> IsEmailExistsAsync(string email);
        #endregion

        #region //Get
        Task<UserInfo> GetUserByEmailAsync(string email);
        Task<UserInfo> GetUserByIdAsync(int userId);
        Task<List<GetUserByKeywordResponse>> GetUserByKeywordAsync(string keyword, int userId);
        Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId);
        Task<List<NotificationResponse>> GetUnreadNotificationsAsync(int userId);
        #endregion

        #region //Add
        Task<ServiceResponse<bool>> AddFriendRequestAsync(AddFriendRequestRequest request);
        #endregion

        #region //Update
        Task<bool> UpdateUserBioAsync(UserInfo user);
        Task<ServiceResponse<bool>> UpdateFriendRequestAsync(FriendRequest request);
        Task<ServiceResponse<bool>> UpdateNotificationStatusAsync(Notification notification);
        Task<ServiceResponse<bool>> UpdateProfileAsync(UserInfo user);
        Task<ServiceResponse<bool>> UpdateAllMessagesAsReadAsync(int userId);
        #endregion

        #region //Delete

        #endregion
    }
}
