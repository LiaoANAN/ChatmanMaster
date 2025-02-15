namespace Chatman.Models.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }

        public static RegisterResponse SuccessResponse(int userId)
        {
            return new RegisterResponse
            {
                Success = true,
                Message = "註冊成功",
                UserId = userId
            };
        }

        public static RegisterResponse ErrorResponse(string message)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}
