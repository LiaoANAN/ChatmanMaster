using Humanizer.Localisation.TimeToClockNotation;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Configuration;

namespace Chatman.Models.DTOs
{
    public class RecentChatsResponse
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int FriendId { get; set; }
        public string FriendName { get; set; }
        public string FriendImage { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public string MediaUrl { get; set; }
        public bool IsRead { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
