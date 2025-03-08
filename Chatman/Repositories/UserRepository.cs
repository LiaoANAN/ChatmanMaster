using Chatman.Interfaces;
using Chatman.Data;
using Chatman.Models;
using Dapper;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Chatman.Models.DTOs;
using System.Diagnostics;
using NuGet.Protocol.Plugins;

namespace Chatman.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private string sql = "";
        private DynamicParameters dynamicParameters = new DynamicParameters();

        public UserRepository(ILogger<UserRepository> logger)
        {
            _logger = logger;
        }

        #region //Get
        public async Task<UserInfo> GetUserByEmailAsync(string email, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT UserId, UserName, Email, Password, Gender, 
                                FORMAT(Birthday, 'yyyy-MM-dd') Birthday, Status, Bio, UserImage
                                , CreateDate, UpdateDate, CreateUserId, UpdateUserId
                        FROM BAS.UserInfo 
                        WHERE Email = @Email";

                var user = await sqlConnection.QueryFirstOrDefaultAsync<UserInfo>(
                    sql, new { Email = email });

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<UserInfo> GetUserByIdAsync(int userId, SqlConnection sqlConnection)
        {
            try
            {
                const string sql = @"SELECT UserId, UserName, Email, Password, Gender, 
                                            Birthday, Status, Bio, UserImage
                                            , CreateDate, UpdateDate, CreateUserId, UpdateUserId
                                    FROM BAS.UserInfo 
                                    WHERE UserId = @UserId";

                var user = await sqlConnection.QueryFirstOrDefaultAsync<UserInfo>(sql, new { UserId = userId });
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by id: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UserInfo>> GetUserByKeywordAsync(string keyword, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT UserId, UserName, Email, Password, Gender, 
                                Birthday, Status, ISNULL(Bio, '') Bio, UserImage
                                , CreateDate, UpdateDate, CreateUserId, UpdateUserId
                        FROM BAS.UserInfo 
                        WHERE Email LIKE @Keyword OR UserName LIKE @Keyword";

                var userResult = (await sqlConnection.QueryAsync<UserInfo>(sql, new { Keyword = $"%{keyword}%", })).ToList();

                return userResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user");
                throw;
            }
        }

        public async Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT a.FriendRelationId, a.UserId, a.FriendId, a.Status
                        , a.CreateDate, a.UpdateDate, a.CreateUserId, a.UpdateUserId
                        , b.UserName, b.Bio, b.UserImage
                        FROM BAS.FriendRelation a 
                        INNER JOIN BAS.UserInfo b ON a.FriendId = b.UserId
                        WHERE a.UserId = @UserId";

                var friends = (await sqlConnection.QueryAsync<FriendRelation>(sql, new { UserId = userId })).ToList();

                return friends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by userId: {userId}", userId);
                throw;
            }
        }

        public async Task<bool> CheckFriendStatusAsync(int userId, int friendId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT TOP 1 1
                        FROM BAS.FriendRelation a 
                        WHERE (a.UserId = @UserId AND a.FriendId = @FriendId)
                        OR (a.UserId = @FriendId AND a.FriendId = @UserId)";

                var result = await sqlConnection.QueryFirstOrDefaultAsync(sql, new { UserId = userId, FriendId = friendId });

                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by userId: {userId}", userId);
                throw;
            }
        }

        public async Task<bool> CheckFriendRequestAsync(int userId, int friendId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT TOP 1 1
                        FROM BAS.FriendRequest a 
                        WHERE a.SenderId = @UserId 
                        AND a.ReceiverId = @FriendId
                        AND a.Status = 'P'";

                var result = await sqlConnection.QueryFirstOrDefaultAsync(sql, new { UserId = userId, FriendId = friendId });

                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by userId: {userId}", userId);
                throw;
            }
        }

        public async Task<List<NotificationResponse>> GetUnreadNotificationsAsync(int userId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT a.NotificationId, a.UserId, a.Type, a.Message, a.RequestId, a.SenderId, a.IsRead, a.Status
                        , FORMAT(a.CreateDate, 'yyyy-MM-dd') CreateDate
                        , b.Email, b.UserName SenderName, b.UserImage SenderImage, b.Gender
                        FROM BAS.Notification a 
                        INNER JOIN BAS.UserInfo b ON a.SenderId = b.UserId
                        WHERE a.UserId = @UserId
                        AND  a.IsRead = 0";

                var result = (await sqlConnection.QueryAsync<NotificationResponse>(sql, new { UserId = userId })).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by userId: {userId}", userId);
                throw;
            }
        }

        public async Task<FriendRequest> GetFriendRequestByIdAsync(int friendRequestId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT a.FriendRequestId, a.SenderId, a.ReceiverId, a.Message, a.Status
                        FROM BAS.FriendRequest a 
                        WHERE a.FriendRequestId = @FriendRequestId";

                var result = await sqlConnection.QueryFirstOrDefaultAsync<FriendRequest>(sql, new { FriendRequestId = friendRequestId });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by friendRequestId: {friendRequestId}", friendRequestId);
                throw;
            }
        }

        public async Task<bool> IsFriendRequestAsync(int friendRequestId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT TOP 1 1
                        FROM BAS.FriendRequest a 
                        WHERE a.FriendRequestId = @FriendRequestId
                        AND a.Status != 'P'";

                var result = await sqlConnection.QueryFirstOrDefaultAsync(sql, new { FriendRequestId = friendRequestId });

                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by friendRequestId: {friendRequestId}", friendRequestId);
                throw;
            }
        }
        #endregion

        #region //Add
        public async Task<int?> AddFriendRequestAsync(FriendRequest request, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"INSERT INTO BAS.FriendRequest (
                            SenderId, ReceiverId, Message, Status,
                            CreateDate, UpdateDate, CreateUserId, UpdateUserId
                        ) 
                        OUTPUT INSERTED.FriendRequestId 
                        VALUES (
                            @SenderId, @ReceiverId, @Message, @Status,
                            @CreateDate, @UpdateDate, @CreateUserId, @UpdateUserId
                        )";

                return await sqlConnection.ExecuteScalarAsync<int>(sql, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {SenderId}", request.SenderId);

                return null;
            }
        }

        public async Task<int?> AddNotificationAsync(Notification notification, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"INSERT INTO BAS.Notification (
                            UserId, Type, Message, RequestId, SenderId, IsRead, Status,
                            CreateDate, UpdateDate, CreateUserId, UpdateUserId
                        ) 
                        OUTPUT INSERTED.NotificationId 
                        VALUES (
                            @UserId, @Type, @Message, @RequestId, @SenderId, @IsRead, @Status,
                            @CreateDate, @UpdateDate, @CreateUserId, @UpdateUserId
                        )";

                return await sqlConnection.ExecuteScalarAsync<int>(sql, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {SenderId}", notification.SenderId);

                return null;
            }
        }

        public async Task<int?> AddFriendRelationAsync(FriendRelation friendRelation, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"INSERT INTO BAS.FriendRelation (
                            UserId, FriendId, Status,
                            CreateDate, UpdateDate, CreateUserId, UpdateUserId
                        ) 
                        OUTPUT INSERTED.FriendRelationId 
                        VALUES (
                            @UserId, @FriendId, @Status,
                            @CreateDate, @UpdateDate, @CreateUserId, @UpdateUserId
                        )";

                return await sqlConnection.ExecuteScalarAsync<int>(sql, friendRelation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", friendRelation.UserId);

                return null;
            }
        }
        #endregion

        #region //Update
        public async Task<bool> UpdateUserAsync(UserInfo user, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE BAS.UserInfo 
                        SET Email = @Email,
                            Password = @Password,
                            Gender = @Gender,
                            Birthday = @Birthday,
                            Status = @Status,
                            UpdateDate = @UpdateDate,
                            UpdateUserId = @UpdateUserId
                        WHERE UserId = @UserId";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, user);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", user.UserId);

                return false;
            }
        }

        public async Task<bool> UpdateUserBioAsync(UserInfo user, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE BAS.UserInfo 
                        SET Bio = @Bio,
                            UpdateDate = @UpdateDate,
                            UpdateUserId = @UpdateUserId
                        WHERE UserId = @UserId";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, user);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", user.UserId);

                return false;
            }
        }

        public async Task<bool> UpdateFriendRequestAsync(FriendRequest request, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE BAS.FriendRequest 
                        SET Status = @Status,
                            UpdateDate = @UpdateDate,
                            UpdateUserId = @UpdateUserId
                        WHERE FriendRequestId = @FriendRequestId";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, request);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {requestId}", request.FriendRequestId);

                return false;
            }
        }

        public async Task<bool> UpdateNotificationStatusAsync(Notification notification, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE BAS.Notification 
                        SET IsRead = @IsRead,
                            Status = @Status,
                            UpdateDate = @UpdateDate,
                            UpdateUserId = @UpdateUserId
                        WHERE NotificationId = @NotificationId";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, notification);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {NotificationId}", notification.NotificationId);

                return false;
            }
        }

        public async Task<bool> UpdateProfileAsync(UserInfo user, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE BAS.UserInfo 
                        SET UserName = @UserName,
                            Bio = @Bio,
                            Gender = @Gender,
                            Birthday = @Birthday,
                            UserImage = @UserImage,
                            UpdateDate = @UpdateDate,
                            UpdateUserId = @UpdateUserId
                        WHERE UserId = @UserId";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, user);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", user.UserId);

                return false;
            }
        }

        public async Task<bool> UpdateAllMessagesAsReadAsync(int userId, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"UPDATE BAS.Notification 
                        SET IsRead = 1,
                            UpdateDate = GETDATE(),
                            UpdateUserId = @UserId
                        WHERE UserId = @UserId
                        AND IsRead = 0";

                var rowsAffected = await sqlConnection.ExecuteAsync(sql, new
                {
                    UserId = userId
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "將訊息標記為已讀時發生錯誤: {userId}", userId);
                throw;
            }
        }
        #endregion

        #region //Delete

        #endregion

        #region //Login
        public async Task<(int, string)> RegisterAsync(UserInfo user, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"INSERT INTO BAS.UserInfo (
                            UserName, Email, Password, Gender, Birthday,
                            Status, CreateDate, CreateUserId
                        ) 
                        OUTPUT INSERTED.UserId 
                        VALUES (
                            @UserName, @Email, @Password, @Gender, @Birthday,
                            @Status, @CreateDate, @CreateUserId
                        )";

                int userId = await sqlConnection.ExecuteScalarAsync<int>(sql, user);

                return (userId, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {Email}", user.Email);
                return (0, ex.Message);
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email, SqlConnection sqlConnection)
        {
            try
            {
                sql = @"SELECT TOP 1 1 FROM BAS.UserInfo WHERE Email = @Email";

                var result = await sqlConnection.QueryAsync(sql, new { Email = email });
                return result.Count() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        #endregion
    }
}
