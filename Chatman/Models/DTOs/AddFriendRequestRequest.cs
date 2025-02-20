namespace Chatman.Models.DTOs
{
    public class AddFriendRequestRequest
    {
        public int SendId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string SenderImage { get; set; }
    }
}
