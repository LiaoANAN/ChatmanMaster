using Chatman.Models;
using Chatman.Models.DTOs;

namespace Chatman.Interfaces
{
    public interface IChatService
    {
        Task<int> SaveMessageAsync(ChatMessage message);
    }
}
