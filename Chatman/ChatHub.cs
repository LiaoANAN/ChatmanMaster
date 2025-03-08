using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Chatman.Models;
using Chatman.Models.DTOs;
using Chatman.Interfaces;
using NuGet.Protocol.Plugins;

public class ChatHub : Hub
{
    // 用於存儲用戶ID和連接ID的映射關係
    private static readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();
    private readonly ILogger<ChatHub> _logger;
    private readonly IChatService _chatService;
    private readonly IUserService _userService;

    public ChatHub(ILogger<ChatHub> logger, IChatService chatService, IUserService userService)
    {
        _logger = logger;
        _chatService = chatService;
        _userService = userService;
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

    // 傳送訊息
    public async Task SendMessage(SendMessageRequest request)
    {
        try
        {
            // 檢查請求中是否包含檔案資訊
            bool isFileMessage = request.MessageType == "file";

            // 儲存訊息到資料庫
            var messageId = await _chatService.SaveMessageAsync(new ChatMessage
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType,
                FileName = request.FileName,
                FileSize = request.FileSize,
                MediaUrl = request.MessageType == "file" ? request.Content : null, // 对于文件类型，URL在MessageContent中
                Status = "A",
                IsRead = false,
                IsDelete = false,
                CreateDate = DateTime.Now,
                CreateUserId = request.SenderId
            });

            // 取得寄送訊息者的資料
            var sender = await _userService.GetUserByIdAsync(request.SenderId);

            // 建立回應物件
            var response = new MessageResponse
            {
                MessageId = messageId,
                SenderId = sender.UserId,
                SenderName = sender.UserName,
                SenderAvatar = sender.UserImage,
                Content = request.Content,
                MessageType = request.MessageType,
                CreateDate = DateTime.Now,
                IsRead = false,
                FileName = request.FileName,
                FileSize = request.FileSize,
                MediaUrl = request.MessageType == "file" ? request.Content : null
            };

            // 如果是檔案訊息，添加檔案相關資訊
            if (isFileMessage)
            {
                response.FileName = request.FileName;
                response.FileSize = request.FileSize;
                response.MediaUrl = request.MediaUrl;
            }

            // 發送給接收者
            await Clients.Group(request.ReceiverId.ToString())
                        .SendAsync("ReceiveMessage", response);

            // 發送回發送者（確認訊息已送達）
            await Clients.Caller.SendAsync("MessageSent", response);

            _logger.LogInformation($"訊息已從 {request.SenderId} 發送給 {request.ReceiverId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送訊息時發生錯誤");
            throw;
        }
    }

    // 通知用戶更新好友列表
    public async Task NotifyFriendListUpdate(int userId)
    {
        try
        {
            // 通知指定用户更新好友列表
            await Clients.Group(userId.ToString()).SendAsync("UpdateFriendList");
            _logger.LogInformation($"Friend list update notification sent to user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending friend list update notification to user {userId}");
            throw;
        }
    }

    // 通知用戶有新訊息
    public async Task NotifyNewMessage(int userId, int fromUserId, string fromUserName)
    {
        try
        {
            await Clients.Group(userId.ToString()).SendAsync("NewMessage", new
            {
                FromUserId = fromUserId,
                FromUserName = fromUserName
            });

            _logger.LogInformation($"新訊息通知已發送給用戶 {userId}，來自 {fromUserName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"發送新訊息通知給用戶 {userId} 時發生錯誤");
            throw;
        }
    }

    // 標記訊息為已讀
    public async Task MarkMessageAsRead(int senderId, int receiverId)
    {
        try
        {
            await _chatService.UpdateMessagesAsReadAsync(senderId, receiverId);

            // 通知發送者，訊息已被讀取
            await Clients.Group(senderId.ToString()).SendAsync("MessagesRead", receiverId);

            _logger.LogInformation($"來自 {senderId} 的訊息已被 {receiverId} 標記為已讀");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"標記訊息為已讀時發生錯誤");
            throw;
        }
    }

    // 通知訊息被收回
    public async Task NotifyMessageRetracted(RetractMessageRequest request)
    {
        try
        {
            // 通知接收者訊息已被收回
            await Clients.Group(request.ReceiverId.ToString())
                .SendAsync("MessageRetracted", new
                {
                    request.MessageId,
                    request.SenderId,
                    request.MessageType
                });

            _logger.LogInformation($"已通知用戶 {request.ReceiverId} 訊息 {request.MessageId} 已被收回");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"通知訊息收回失敗: {request.MessageId}");
            throw;
        }
    }

    // 發送回覆訊息
    public async Task SendReplyMessage(ReplyMessageRequest request)
    {
        try
        {
            // 儲存訊息到資料庫
            var messageId = await _chatService.SaveMessageAsync(new ChatMessage
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType,
                Status = "A",
                IsRead = false,
                CreateDate = DateTime.Now,
                CreateUserId = request.SenderId,
                // 回覆訊息資訊
                ReplyToMessageId = request.ReplyTo.MessageId,
                ReplyToSenderName = request.ReplyTo.SenderName,
                ReplyToContent = request.ReplyTo.Content,
                ReplyToMessageType = request.ReplyTo.MessageType,
                ReplyToImageUrl = request.ReplyTo.ImageUrl,
                ReplyToFileName = request.ReplyTo.FileName
            });

            // 取得寄送訊息者的資料
            var sender = await _userService.GetUserByIdAsync(request.SenderId);

            // 建立回應物件
            var response = new MessageResponse
            {
                MessageId = messageId,
                SenderId = sender.UserId,
                SenderName = sender.UserName,
                SenderAvatar = sender.UserImage,
                Content = request.Content,
                MessageType = request.MessageType,
                CreateDate = DateTime.Now,
                IsRead = false,
                // 回覆訊息資訊
                ReplyTo = request.ReplyTo
            };

            // 發送給接收者
            await Clients.Group(request.ReceiverId.ToString())
                        .SendAsync("ReceiveReplyMessage", response);

            // 發送回發送者（確認訊息已送達）
            await Clients.Caller.SendAsync("MessageSent", response);

            _logger.LogInformation($"回覆訊息已從 {request.SenderId} 發送給 {request.ReceiverId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送回覆訊息時發生錯誤");
            throw;
        }
    }
}