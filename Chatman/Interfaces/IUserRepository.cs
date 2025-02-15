using Chatman.Models;
using Chatman.Models.DTOs;

namespace Chatman.Interfaces
{
    public interface IUserRepository
    {
        Task<UserInfo> GetByEmailAsync(string email);
        Task<UserInfo> GetByIdAsync(int userId);
        Task<bool> UpdateUserAsync(UserInfo user);
        Task<int> RegisterAsync(UserInfo user);
        Task<bool> IsEmailExistsAsync(string email);
    }
}
