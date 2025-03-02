using Chatman.Interfaces;
using Chatman.Models;
using Chatman.Models.DTOs;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Chatman.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ILogger<ChatRepository> _logger;
        private string sql = "";
        private DynamicParameters dynamicParameters = new DynamicParameters();

        public ChatRepository(ILogger<ChatRepository> logger)
        {
            _logger = logger;
        }

        #region //Get
        public async Task<List<ChatMessage>> GetChatHistoryAsync(int userId, int friendId, int pageSize, int pageNumber, SqlConnection sqlConnection)
        {
            try
            {
                int offset = (pageNumber - 1) * pageSize;

                sql = @"SELECT MessageId, SenderId, ReceiverId, MessageType, Content, 
                               MediaUrl, IsRead, IsDelete, Status, CreateDate
                        FROM CHAT.Message
                        WHERE ((SenderId = @UserId AND ReceiverId = @FriendId) 
                           OR (SenderId = @FriendId AND ReceiverId = @UserId))
                          AND IsDelete = 0
                        ORDER BY CreateDate DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

                var messages = await sqlConnection.QueryAsync<ChatMessage>(sql, new
                {
                    UserId = userId,
                    FriendId = friendId,
                    Offset = offset,
                    PageSize = pageSize
                });

                return messages.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取聊天記錄時發生錯誤: {UserId}, {FriendId}", userId, friendId);
                throw;
            }
        }

        public async Task<int> GetUnreadMessagesCountAsync(int userId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT COUNT(*) 
                        FROM CHAT.Message 
                        WHERE ReceiverId = @UserId 
                          AND IsRead = 0
                          AND IsDelete = 0
                          AND Status = 'A'";

                return await sqlConnection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤: {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadMessagesCountFromFriendAsync(int userId, int friendId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT COUNT(*) 
                        FROM CHAT.Message 
                        WHERE ReceiverId = @UserId 
                          AND SenderId = @FriendId
                          AND IsRead = 0
                          AND IsDelete = 0
                          AND Status = 'A'";

                return await sqlConnection.ExecuteScalarAsync<int>(sql, new { UserId = userId, FriendId = friendId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未讀訊息數量時發生錯誤: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<RecentChatsResponse>> GetRecentChatsAsync(int userId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"WITH RecentMessages AS (
                            SELECT 
                                m.*,
                                ROW_NUMBER() OVER (PARTITION BY 
                                    CASE 
                                        WHEN SenderId = @UserId THEN ReceiverId 
                                        ELSE SenderId 
                                    END 
                                    ORDER BY CreateDate DESC) AS RowNum
                            FROM CHAT.Message m
                            WHERE (SenderId = @UserId OR ReceiverId = @UserId)
                            AND IsDelete = 0
                        )
                        SELECT 
                            rm.MessageId,
                            rm.MessageType,
                            rm.Content,
                            rm.MediaUrl,
                            rm.IsRead,
                            rm.CreateDate,
                            -- 確定對話的另一方
                            CASE 
                                WHEN rm.SenderId = @UserId THEN rm.ReceiverId
                                ELSE rm.SenderId
                            END AS FriendId,
                            -- 計算該用戶的未讀訊息數量
                            (SELECT COUNT(*) 
                             FROM CHAT.Message 
                             WHERE SenderId = CASE WHEN rm.SenderId = @UserId THEN rm.ReceiverId ELSE rm.SenderId END
                             AND ReceiverId = @UserId 
                             AND IsRead = 0
                             AND IsDelete = 0) AS UnreadCount,
                            -- 獲取好友的用戶信息
                            u.UserName FriendName,
                            u.UserImage FriendImage
                        FROM RecentMessages rm
                        INNER JOIN BAS.UserInfo u ON 
                            CASE 
                                WHEN rm.SenderId = @UserId THEN rm.ReceiverId
                                ELSE rm.SenderId
                            END = u.UserId
                        WHERE RowNum = 1
                        ORDER BY rm.CreateDate DESC;";

                var recentChats = await sqlConnection.QueryAsync<RecentChatsResponse>(sql, new
                {
                    UserId = userId,
                });

                return recentChats.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取聊天記錄時發生錯誤: {userId}", userId);
                throw;
            }
        }

        public async Task<List<RecentChatsResponse>> GetRecentChatsByKeywordAsync(string keyword, int userId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT a.MessageId, a.MessageType, a.Content , a.MediaUrl, a.SenderId, a.CreateDate
                        , b.UserId FriendId, b.UserName FriendName, b.UserImage FriendImage
                        FROM CHAT.Message a 
                        INNER JOIN BAS.UserInfo b ON a.ReceiverId = b.UserId
                        WHERE (a.SenderId = @UserId OR a.ReceiverId = @UserId)
                        AND Content LIKE @keyword";

                var recentChats = await sqlConnection.QueryAsync<RecentChatsResponse>(sql, new
                {
                    UserId = userId,
                    Keyword = $"%{keyword}%",
                });

                return recentChats.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取聊天記錄時發生錯誤: {userId}", userId);
                throw;
            }
        }
        #endregion

        #region //Add
        public async Task<int> SaveMessageAsync(ChatMessage message, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"INSERT INTO CHAT.Message (
                            SenderId, ReceiverId, MessageType, Content, MediaUrl, IsRead, IsDelete, Status,
                            CreateDate
                        ) 
                        OUTPUT INSERTED.MessageId 
                        VALUES (
                            @SenderId, @ReceiverId, @MessageType, @Content, @MediaUrl, @IsRead, @IsDelete, @Status,
                            @CreateDate
                        )";

                return await sqlConnection.ExecuteScalarAsync<int>(sql, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {SenderId}", message.SenderId);

                return 0;
            }
        }
        #endregion

        #region //Update
        public async Task<bool> UpdateMessagesAsReadAsync(int senderId, int receiverId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE CHAT.Message
                        SET IsRead = 1
                        WHERE SenderId = @SenderId 
                          AND ReceiverId = @ReceiverId
                          AND IsRead = 0";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, new
                {
                    SenderId = senderId,
                    ReceiverId = receiverId
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "將訊息標記為已讀時發生錯誤: {SenderId}, {ReceiverId}", senderId, receiverId);
                throw;
            }
        }
        #endregion

        #region //Delete

        #endregion
    }
}
