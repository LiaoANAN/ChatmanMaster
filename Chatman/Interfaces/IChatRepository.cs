using Chatman.Models;
using Microsoft.Data.SqlClient;

namespace Chatman.Interfaces
{
    public interface IChatRepository
    {
        #region //Get

        #endregion

        #region //Add
        Task<int> SaveMessageAsync(ChatMessage message, SqlConnection sqlConnection);
        #endregion

        #region //Update

        #endregion

        #region //Delete

        #endregion
    }
}
