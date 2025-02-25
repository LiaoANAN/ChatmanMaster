namespace Chatman.Models.DTOs
{
    public class MessageResponse
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string MessageContent { get; set; }
        public string MessageType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
