using Chatman.Interfaces;
using Chatman.Models.DTOs;
using Chatman.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Chatman.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService (IUserRepository userRepository, IConfiguration configuration, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "使用者不存在"
                    };
                }

                if (user.Status != "A")
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "帳號已被停用"
                    };
                }

                if (!await ValidatePasswordAsync(request.Password, user.Password))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "密碼錯誤"
                    };
                }

                // 生成 JWT Token
                var token = GenerateJwtToken(user);

                return new LoginResponse
                {
                    Success = true,
                    Message = "登入成功",
                    Token = token,
                    UserInfo = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    Message = "登入過程發生錯誤"
                };
            }
        }

        public async Task<UserInfo> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserCode)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<bool> ValidatePasswordAsync(string hashedPassword, string inputPassword)
        {
            try
            {
                // 在實際應用中，這裡應該使用加密後的密碼比對
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                    var computedHash = Convert.ToBase64String(hashedBytes);
                    return hashedPassword == computedHash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password validation failed");
                throw;
            }
        }
    }
}
