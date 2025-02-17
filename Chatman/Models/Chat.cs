namespace Chatman.Models
{
    public class Chat
    {
        public class UserConnection
        {
            public int UserId { get; set; }
            public string ConnectionId { get; set; }
            public DateTime LastActivity { get; set; }
        }
    }
}
