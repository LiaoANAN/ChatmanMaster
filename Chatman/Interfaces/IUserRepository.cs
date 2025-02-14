using Chatman.Models;

namespace Chatman.Interfaces
{
    public interface IUserRepository
    {
        Task<UserInfo> GetByEmailAsync(string email);
        Task<UserInfo> GetByIdAsync(int userId);
        Task<bool> UpdateUserAsync(UserInfo user);
        Task<bool> CreateUserAsync(UserInfo user);
    }
}
