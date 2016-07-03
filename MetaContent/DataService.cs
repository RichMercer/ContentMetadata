using System.Data;
using System.Data.SqlClient;
using Telligent.Evolution.Extensibility.Api.Version1;

namespace MetaContent
{
    internal static class DataService
    {
        #region Helper Methods

        private static SqlConnection GetSqlConnection()
        {
            return PublicApi.DatabaseConnections.GetConnection("SiteSqlServer");
        }

        private static SqlCommand CreateSprocCommand(string sprocName, SqlConnection connection)
        {
            return new SqlCommand("dbo." + sprocName, connection) { CommandType = CommandType.StoredProcedure };
        }

        #endregion

    }
}
