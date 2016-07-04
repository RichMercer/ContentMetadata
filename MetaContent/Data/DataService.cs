using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telligent.Evolution.Components;
using Telligent.Evolution.Extensibility.Api.Version1;

namespace MetaContent.Data
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

        public static bool IsInstalled()
        {
            const string statement = @"SELECT 1 FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[custom_MetaData]') AND TYPE IN (N'U')";

            int value;
            using (var connection = GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(statement, connection))
                {
                    var result = command.ExecuteScalar();
                    
                    int.TryParse(result?.ToString(), out value);
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

        public static ContentMeta Get(Guid contentId, string key)
        {
            ContentMeta item = null;

            using (var connection = GetSqlConnection())
            {
                using (var command = CreateSprocCommand("[custom_MetaData_Get]", connection))
                {
                    command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;
                    command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;

                    connection.Open();

                    using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            item = Populate(reader);
                        }
                    }
                }
            }

            return item;
        }

        public static IList<ContentMeta> List(Guid contentId)
        {
            var items = new List<ContentMeta>();

            using (var connection = GetSqlConnection())
            {
                using (var command = CreateSprocCommand("[custom_MetaData_List]", connection))
                {
                    command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;
                    
                    connection.Open();

                    using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            items.Add(Populate(dr));
                        }
                        // Done with the reader and the connection
                        dr.Close();
                    }

                    connection.Close();
                }
            }

            return items;
        }

        public static void Delete(Guid contentId)
        {
            using (var connection = GetSqlConnection())
            {
                using (var command = CreateSprocCommand("[custom_MetaData_Delete]", connection))
                {
                    command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static void Set(Guid contentId, Guid contentTypeId, string key, string value)
        {
            using (var connection = GetSqlConnection())
            {
                using (var command = CreateSprocCommand("[custom_MetaData_Set]", connection))
                {
                    command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;
                    command.Parameters.Add("@ContentTypeId", SqlDbType.UniqueIdentifier).Value = contentTypeId;
                    command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;
                    command.Parameters.Add("@DataValue", SqlDbType.Text).Value = value;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private static ContentMeta Populate(IDataReader reader)
        {
            var article = new ContentMeta(
                reader["ContentId"] == null ? Guid.Empty : new Guid(reader["ContentId"].ToString()),
                reader["ContentTypeId"] == null ? Guid.Empty : new Guid(reader["ContentTypeId"].ToString()),
                reader["DataKey"].ToString(),
                reader["DataValue"].ToString()
                );

            return article;
        }
    }
}
