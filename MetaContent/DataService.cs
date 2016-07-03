using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telligent.Evolution.Components;
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

        private static bool HasRequiredPermissions()
        {
            const string statement = @"IF IS_ROLEMEMBER ('db_owner') = 1
                                    BEGIN
                                        SELECT 1
                                    END
                                    ELSE
                                    BEGIN
	                                    SELECT 0
                                    END";

            int value;
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(statement, connection))
                {
                    int.TryParse(command.ExecuteScalar().ToString(), out value);
                }
                connection.Close();
            }

            return Convert.ToBoolean(value);
        }

        #endregion

        #region IInstallablePlugin Methods

        internal static void Install()
        {
            if (!HasRequiredPermissions())
            {
                EventLogs.Warn("Unable to install SQL scripts for plugin", "MetaContent", 3660);
                return;
            }

            using (var connection = GetSqlConnection())
            {
                connection.Open();
                foreach (var statement in GetBatchedStatementsFromSql())
                {
                    using (var command = new SqlCommand(statement, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        private static IEnumerable<string> GetBatchedStatementsFromSql()
        {
            var sqlScript = GetStatementsFromSql();
            return Regex.Split(sqlScript, @"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline).Select(statement => Regex.Replace(statement, @"(?:^SET\s+.*?$|\/\*.*?\*\/|--.*?$)", "\r\n",
                RegexOptions.IgnoreCase | RegexOptions.Multiline).Trim()).Where(sanitizedStatement => sanitizedStatement.Length > 0);
        }

        private static string GetStatementsFromSql()
        {
            var stream = typeof(DataService).Assembly.GetManifestResourceStream("MetaContent.Resources.Sql.Install.sql");
            if (stream == null || stream.Length == 0)
                return string.Empty;

            using (stream)
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                var text = Encoding.UTF8.GetString(data);
                return text[0] > 255 ? text.Substring(1) : text;
            }
        }

        #endregion


    }
}
