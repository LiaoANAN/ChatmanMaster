namespace Chatman.Models
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public string MediaUrl { get; set; }
        public bool IsRead { get; set; }
        public bool IsDelete { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreateUserId { get; set; }
        public int? UpdateUserId { get; set; }
    }
}
