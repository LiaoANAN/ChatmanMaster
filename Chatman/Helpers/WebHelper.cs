using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Chatman.Models;
using System.Text;
using System.Security.Cryptography;

namespace Chatman.Helpers
{
    public static class WebHelper
    {
        public static UserInfo GetCurrentUser(this HttpContext context)
        {
            try
            {
                // 從 cookie 中獲取 token
                var token = context.Request.Cookies["auth_token"];
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // 從 token claims 中提取用戶資訊
                var userInfo = new UserInfo
                {
                    UserId = int.Parse(jwtToken.Claims.First(x => x.Type == "userId").Value),
                    UserName = jwtToken.Claims.First(x => x.Type == "userName").Value,
                    Email = jwtToken.Claims.First(x => x.Type == "email").Value,
                    Status = jwtToken.Claims.First(x => x.Type == "status").Value
                };

                return userInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
