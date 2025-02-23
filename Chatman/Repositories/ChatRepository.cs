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

        #endregion

        #region //Add
        public async Task<int> SaveMessageAsync(ChatMessage message, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"INSERT INTO CHAT.Message (
                            SenderId, ReceiverId, MessageType, Content, MediaUrl, IsRead, IsDelete, Status
                            CreateDate, UpdateDate, CreateUserId, UpdateUserId
                        ) 
                        OUTPUT INSERTED.MessageId 
                        VALUES (
                            @SenderId, @ReceiverId, @MessageType, @Content, @MediaUrl, @IsRead, @IsDelete, @Status
                            @CreateDate, @UpdateDate, @CreateUserId, @UpdateUserId
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

        #endregion

        #region //Delete

        #endregion
    }
}
