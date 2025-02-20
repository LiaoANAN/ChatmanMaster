namespace Chatman.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string Bio { get; set; }
        public string UserImage { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreateUserId { get; set; }
        public int? UpdateUserId { get; set; }
    }

    public class FriendRelation
    {
        public int FriendRelationId { get; set; }
        public int UserId { get; set; }
        public int FriendId { get; set; }
        public string UserName { get; set; }
        public string Bio { get; set; }
        public string UserImage { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreateUserId { get; set; }
        public int? UpdateUserId { get; set; }
    }

    public class FriendRequest
    {
        public int FriendRequestId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreateUserId { get; set; }
        public int? UpdateUserId { get; set; }
    }

    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }  // 接收通知的用戶
        public string Type { get; set; }  // 通知類型 (friendRequest, system 等)
        public string Message { get; set; }
        public int? RequestId { get; set; }  // 關聯的請求ID
        public int? SenderId { get; set; }   // 發送者ID
        public bool IsRead { get; set; }
        public string Status { get; set; }  // A: 活動, D: 已刪除
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreateUserId { get; set; }
        public int? UpdateUserId { get; set; }
    }
}
