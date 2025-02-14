using Chatman.Models.DTOs;
using Chatman.Models;

namespace Chatman.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserInfo> GetUserByEmailAsync(string email);
        Task<bool> ValidatePasswordAsync(string hashedPassword, string inputPassword);
    }
}
