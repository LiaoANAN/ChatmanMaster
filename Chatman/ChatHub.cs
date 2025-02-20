using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Chatman.Models;

public class ChatHub : Hub
{
    // 用於存儲用戶ID和連接ID的映射關係
    private static readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    // 用戶註冊/登入時調用
    public async Task RegisterUser(int userId)
    {
        try
        {
            var connectionId = Context.ConnectionId;

            // 將連接ID添加到用戶的連接集合中
            _userConnections.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (_, hashSet) =>
                {
                    hashSet.Add(connectionId);
                    return hashSet;
                });

            // 將連接ID關聯到用戶ID
            await Groups.AddToGroupAsync(connectionId, userId.ToString());

            // 通知其他用戶該用戶上線
            await Clients.Others.SendAsync("UserOnline", userId);

            _logger.LogInformation($"User {userId} registered with connection {connectionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in RegisterUser for userId {userId}");
            throw;
        }
    }

    // 當連接斷開時
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var connectionId = Context.ConnectionId;

            // 找到擁有此連接ID的用戶
            var userKvp = _userConnections.FirstOrDefault(kvp =>
                kvp.Value.Contains(connectionId));

            if (userKvp.Value != null)
            {
                int userId = userKvp.Key;

                // 從用戶的連接集合中移除此連接ID
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(connectionId);

                    // 如果用戶沒有其他活動連接，則移除整個用戶記錄
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                        // 通知其他用戶該用戶離線
                        await Clients.Others.SendAsync("UserOffline", userId);
                    }
                }

                await Groups.RemoveFromGroupAsync(connectionId, userId.ToString());
            }

            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation($"Connection {connectionId} disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync");
            throw;
        }
    }

    // 獲取在線用戶ID列表
    public List<int> GetOnlineUserIds()
    {
        return _userConnections.Keys.ToList();
    }

    // 新增用於發送好友請求通知的方法
    public async Task SendFriendRequestNotification(int receiverId, object notification)
    {
        try
        {
            // 使用接收者的user id作為group名稱
            await Clients.Group(receiverId.ToString()).SendAsync("ReceiveFriendRequest", notification);
            _logger.LogInformation($"Friend request notification sent to user {receiverId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending friend request notification to user {receiverId}");
            throw;
        }
    }

    // 發送好友請求通知
    public async Task SendFriendRequest(int requestId, string senderName, string message, int receiverId)
    {
        try
        {
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveFriendRequest", new
            {
                RequestId = requestId,
                SenderName = senderName,
                Message = message
            });

            _logger.LogInformation($"Friend request sent from {senderName} to user {receiverId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending friend request from {senderName} to {receiverId}");
            throw;
        }
    }

    // 發送好友請求回應通知
    public async Task SendFriendRequestResponse(int userId, string userName, bool accepted)
    {
        try
        {
            string eventName = accepted ? "FriendRequestAccepted" : "FriendRequestRejected";
            await Clients.User(userId.ToString()).SendAsync(eventName, new
            {
                UserId = userId,
                UserName = userName
            });

            _logger.LogInformation($"Friend request response sent to user {userId}, accepted: {accepted}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending friend request response to {userId}");
            throw;
        }
    }
}