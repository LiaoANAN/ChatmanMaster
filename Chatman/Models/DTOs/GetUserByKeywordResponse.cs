namespace Chatman.Models.DTOs
{
    public class GetUserByKeywordResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; } = "";
        public string? UserImage { get; set; }  // 頭像URL
        public string FriendStatus { get; set; }  // none, pending, friends, blocked
        public DateTime CreateDate { get; set; }
        public string Status { get; set; }  // A: 正常, D: 停用
    }
}
