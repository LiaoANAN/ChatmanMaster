namespace Chatman.Models.DTOs
{
    public class ReplyMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } // text, image, file
        public ReplyInfo ReplyTo { get; set; }
    }

    public class ReplyInfo
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
        public string ImageUrl { get; set; } // 如果是回覆圖片訊息
        public string FileName { get; set; } // 如果是回覆檔案訊息
    }
}
