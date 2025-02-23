using Azure.Core;
using Chatman.Data;
using Chatman.Interfaces;
using Chatman.Models;
using Chatman.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace Chatman.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IDatabaseConnection _db;

        public ChatService(IChatRepository chatRepository, IConfiguration configuration, ILogger<UserService> logger, IHubContext<ChatHub> hubContext, IDatabaseConnection db)
        {
            _chatRepository = chatRepository;
            _configuration = configuration;
            _hubContext = hubContext;
            _logger = logger;
            _db = db;
        }

        #region //Get

        #endregion

        #region //Add
        public async Task<int> SaveMessageAsync(ChatMessage message)
        {
            try
            {
                using TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                {
                    using (SqlConnection sqlConnection = _db.CreateConnection())
                    {
                        var result = await _chatRepository.SaveMessageAsync(message, sqlConnection);
                        transactionScope.Complete();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
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
