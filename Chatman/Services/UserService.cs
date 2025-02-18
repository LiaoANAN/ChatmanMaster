using Chatman.Interfaces;
using Chatman.Models.DTOs;
using Chatman.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Azure.Core;
using Chatman.Helpers;

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
                if (request.Email == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "信箱不能為空"
                    };
                }

                if (request.Password == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "密碼不能為空"
                    };
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);

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

                if (!ValidatePasswordAsync(request.Password, user.Password))
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
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<UserInfo> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<List<UserInfo>> GetUserInfoAsync(string keyword)
        {
            return await _userRepository.GetUserInfoAsync(keyword);
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.UserId.ToString()),
                new Claim("userName", user.UserName),
                new Claim("email", user.Email),
                new Claim("status", user.Status),
                // 如果需要，可以添加更多 claims
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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

        public bool ValidatePasswordAsync(string inputPassword, string hashedPassword)
        {
            var hashedInput = WebHelper.HashPassword(inputPassword);
            return hashedPassword == hashedInput;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                //確認信箱是否重複
                if (await IsEmailExistsAsync(request.Email))
                {
                    return RegisterResponse.ErrorResponse("此電子郵件已被註冊");
                }

                UserInfo user = new UserInfo
                {
                    UserName = request.Username,
                    Email = request.Email,
                    Password = WebHelper.HashPassword(request.Password),
                    Gender = request.Gender,
                    Birthday = request.Birthday,
                    Status = "A",
                    CreateDate = DateTime.Now,
                    CreateUserId = 0
                };

                int userId = await _userRepository.RegisterAsync(user);

                if (userId > 0)
                {
                    return new RegisterResponse()
                    {
                        Success = true,
                        Message = "註冊成功",
                        UserId = userId
                    };
                }

                return RegisterResponse.ErrorResponse("註冊失敗!");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return RegisterResponse.ErrorResponse(ex.Message);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userRepository.IsEmailExistsAsync(email);
        }

        public async Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId)
        {
            return await _userRepository.GetFriendsByUserIdAsync(userId);
        }

        public async Task<bool> UpdateUserBio(UserInfo user)
        {
            return await _userRepository.UpdateUserBio(user);
        }
    }
}
