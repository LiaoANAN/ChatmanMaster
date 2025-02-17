using Chatman.Models;
using Chatman.Models.DTOs;

namespace Chatman.Interfaces
{
    public interface IUserRepository
    {
        Task<UserInfo> GetUserByEmailAsync(string email);
        Task<UserInfo> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(UserInfo user);
        Task<bool> UpdateUserBio(UserInfo user);
        Task<int> RegisterAsync(UserInfo user);
        Task<bool> IsEmailExistsAsync(string email);
        Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId);
    }
}
