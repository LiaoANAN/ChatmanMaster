using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using static Chatman.Models.Chat;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, UserConnection> OnlineUsers
        = new ConcurrentDictionary<string, UserConnection>();

    public async Task RegisterUser(int userId)
    {
        var connection = new UserConnection
        {
            UserId = userId,
            ConnectionId = Context.ConnectionId,
            LastActivity = DateTime.UtcNow
        };

        // 當用戶註冊時，通知所有客戶端
        if (OnlineUsers.TryAdd(Context.ConnectionId, connection))
        {
            await Clients.Others.SendAsync("UserOnline", userId);
        }
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // 當用戶連接時
    public override async Task OnConnectedAsync()
    {
        // 當用戶斷開連接時，通知所有客戶端
        if (OnlineUsers.TryRemove(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.Others.SendAsync("UserOffline", userConnection.UserId);
        }

        // 等待 RegisterUser 方法被調用
        await base.OnConnectedAsync();
    }

    // 當用戶斷開連接時
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (OnlineUsers.TryRemove(Context.ConnectionId, out UserConnection userConnection))
        {
            await Clients.All.SendAsync("UserOffline", userConnection.UserId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // 獲取在線用戶ID列表
    public async Task<List<int>> GetOnlineUserIds()
    {
        return OnlineUsers.Values
            .Select(x => x.UserId)
            .Distinct()
            .ToList();
    }
}