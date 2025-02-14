namespace Chatman.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserCode { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreateUserId { get; set; }
        public int? UpdateUserId { get; set; }
    }
}
