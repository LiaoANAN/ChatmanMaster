using Chatman.Interfaces;
using Chatman.Data;
using Chatman.Models;
using Dapper;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Chatman.Models.DTOs;

namespace Chatman.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _db;
        private readonly ILogger<UserRepository> _logger;
        private string sql = "";

        public UserRepository(IDatabaseConnection db, ILogger<UserRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<UserInfo> GetByEmailAsync(string email)
        {
            try
            {
                using var connection = _db.CreateConnection();
                sql = @"SELECT UserId, UserName, Email, Password, Gender, 
                                Birthday, Status, Bio, CreateDate, UpdateDate, 
                                CreateUserId, UpdateUserId
                        FROM BAS.UserInfo 
                        WHERE Email = @Email";

                var user = await connection.QueryFirstOrDefaultAsync<UserInfo>(
                    sql, new { Email = email });
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<UserInfo> GetByIdAsync(int userId)
        {
            try
            {
                using var connection = _db.CreateConnection();
                const string sql = @"
                    SELECT UserId, UserCode, Email, Password, Gender, 
                           Birthday, Status, CreateDate, UpdateDate, 
                           CreateUserId, UpdateUserId
                    FROM BAS.UserInfo 
                    WHERE UserId = @UserId";

                var user = await connection.QueryFirstOrDefaultAsync<UserInfo>(
                    sql, new { UserId = userId });
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by id: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(UserInfo user)
        {
            try
            {
                using var connection = _db.CreateConnection();
                await connection.OpenAsync();
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    const string sql = @"
                                        UPDATE BAS.UserInfo 
                                        SET Email = @Email,
                                            Password = @Password,
                                            Gender = @Gender,
                                            Birthday = @Birthday,
                                            Status = @Status,
                                            UpdateDate = @UpdateDate,
                                            UpdateUserId = @UpdateUserId
                                        WHERE UserId = @UserId";

                    user.UpdateDate = DateTime.Now;
                    var rowsAffected = await connection.ExecuteAsync(sql, user);

                    transactionScope.Complete();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", user.UserId);
                throw;
            }
        }

        public async Task<int> RegisterAsync(UserInfo user)
        {
            try
            {
                using var connection = _db.CreateConnection();
                await connection.OpenAsync();

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    const string sql = @"
                                        INSERT INTO BAS.UserInfo (
                                            UserName, Email, Password, Gender, Birthday,
                                            Status, CreateDate, CreateUserId
                                        ) 
                                        OUTPUT INSERTED.UserId 
                                        VALUES (
                                            @UserName, @Email, @Password, @Gender, @Birthday,
                                            @Status, @CreateDate, @CreateUserId
                                        )";

                    int userId = await connection.ExecuteScalarAsync<int>(sql, user);

                    transactionScope.Complete();

                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {Email}", user.Email);
                throw;
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                using var connection = _db.CreateConnection();
                sql = @"SELECT TOP 1 1 FROM BAS.UserInfo WHERE Email = @Email";

                var result = await connection.QueryAsync(sql, new { Email = email });
                return result.Count() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<List<FriendRelation>> GetFriendsByUserIdAsync(int userId)
        {
            try
            {
                using var connection = _db.CreateConnection();
                sql = @"SELECT a.FriendRelationId, a.UserId, a.FriendId, a.Status
                        , a.CreateDate, a.UpdateDate, a.CreateUserId, a.UpdateUserId
                        , b.UserName, b.Bio
                        FROM BAS.FriendRelation a 
                        INNER JOIN BAS.UserInfo b ON a.FriendId = b.UserId
                        WHERE a.UserId = @UserId";

                var friends = (await connection.QueryAsync<FriendRelation>(sql, new { UserId = userId })).ToList();
                
                return friends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by userId: {userId}", userId);
                throw;
            }
        }
    }
}
