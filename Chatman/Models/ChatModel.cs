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
        public string FileName { get; set; }
        public int? FileSize { get; set; }
        public string FileUrl { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string ReplyToSenderName { get; set; }
        public string ReplyToContent { get; set; }
        public string ReplyToMessageType { get; set; }
        public string ReplyToImageUrl { get; set; }
        public string ReplyToFileName { get; set; }
    }
}
