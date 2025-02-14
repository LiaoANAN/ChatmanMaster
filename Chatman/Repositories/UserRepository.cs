using Chatman.Interfaces;
using Chatman.Data;
using Chatman.Models;
using Dapper;
using System.Transactions;
using Microsoft.Data.SqlClient;

namespace Chatman.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _db;
        private readonly ILogger<UserRepository> _logger;

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
                const string sql = @"
                    SELECT UserId, UserCode, Email, Password, Gender, 
                           Birthday, Status, CreateDate, UpdateDate, 
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

        public async Task<bool> CreateUserAsync(UserInfo user)
        {
            try
            {
                using var connection = _db.CreateConnection();
                await connection.OpenAsync();

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    const string sql = @"
                    INSERT INTO BAS.UserInfo (
                        UserCode, Email, Password, Gender, Birthday,
                        Status, CreateDate, CreateUserId
                    ) VALUES (
                        @UserCode, @Email, @Password, @Gender, @Birthday,
                        @Status, @CreateDate, @CreateUserId
                    )";

                    user.CreateDate = DateTime.Now;
                    user.Status = "A"; // Active
                    var rowsAffected = await connection.ExecuteAsync(sql, user);

                    transactionScope.Complete();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {Email}", user.Email);
                throw;
            }
        }
    }
}
