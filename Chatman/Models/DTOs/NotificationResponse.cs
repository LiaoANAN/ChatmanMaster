namespace Chatman.Models.DTOs
{
    public class NotificationResponse
    {
        public int RequestId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderImage { get; set; }
        public string Message { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
