namespace Chatman.Models.DTOs
{
    public class SendMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "T";  // 預設為文字訊息
    }
}
