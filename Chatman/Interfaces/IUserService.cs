using Chatman.Models.DTOs;
using Chatman.Models;

namespace Chatman.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserInfo> GetUserByEmailAsync(string email);
        Task<UserInfo> GetUserByIdAsync(int userId);
        Task<List<UserInfo>> GetUserInfoAsync(string keyword);
        bool ValidatePasswordAsync(string hashedPassword, string inputPassword);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request); 
        Task<bool> IsEmailExistsAsync(string email);
        Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId);
        Task<bool> UpdateUserBio(UserInfo user);
    }
}
