namespace Chatman.Models.DTOs
{
    public class RetractMessageRequest
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageType { get; set; }
    }
}
