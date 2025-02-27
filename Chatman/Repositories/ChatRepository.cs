using Chatman.Interfaces;
using Chatman.Models;
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
