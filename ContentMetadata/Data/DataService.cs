﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Api.Version1;

namespace ContentMetadata.Data;

internal static class DataService
{
	#region Helper Methods

	private static SqlConnection GetSqlConnection()
	{
		return Apis.Get<IDatabaseConnections>().GetConnection("SiteSqlServer");
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
		}

		return Convert.ToBoolean(value);
	}

	#endregion

	#region IInstallablePlugin Methods

	internal static void Install()
	{
		if (!HasRequiredPermissions())
		{
			Apis.Get<IEventLog>().Write("Unable to install SQL scripts for plugin", new EventLogEntryWriteOptions { EventType = "Warning", Category = "ContentMetadata", EventId = 3660 });
			return;
		}

		using var connection = GetSqlConnection();
		connection.Open();
		foreach (var statement in GetBatchedStatementsFromSql())
		{
			using var command = new SqlCommand(statement, connection);
			command.ExecuteNonQuery();
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
		var stream = typeof(DataService).Assembly.GetManifestResourceStream("ContentMetadata.Resources.Sql.Install.sql");
		if (stream == null || stream.Length == 0)
			return string.Empty;

		using (stream)
		{
			var data = new byte[stream.Length];
			_ = stream.Read(data, 0, data.Length);
			var text = Encoding.UTF8.GetString(data);
			return text[0] > 255 ? text.Substring(1) : text;
		}
	}

	#endregion

	public static List<ContentMetadata> List(Guid contentId)
	{
		var items = new List<ContentMetadata>();

		using var connection = GetSqlConnection();
		using var command = CreateSprocCommand("[custom_MetaData_List]", connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;

		connection.Open();

		using (var dr = command.ExecuteReader())
		{
			while (dr.Read())
			{
				items.Add(Populate(dr));
			}
			// Done with the reader and the connection
			dr.Close();
		}

		connection.Close();

		return items;
	}

	public static List<ContentMetadata> ListContent(string key, string value)
	{
		var items = new List<ContentMetadata>();

		using var connection = GetSqlConnection();
		using var command = CreateSprocCommand("[custom_Metadata_List_Key]", connection);
		command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;
		command.Parameters.Add("@DataValue", SqlDbType.NVarChar).Value = value;
		connection.Open();

		using (var dr = command.ExecuteReader())
		{
			while (dr.Read())
			{
				items.Add(Populate(dr));
			}
			// Done with the reader and the connection
			dr.Close();
		}

		connection.Close();

		return items;
	}


	public static void Delete(Guid contentId)
	{
		using var connection = GetSqlConnection();
		using var command = CreateSprocCommand("[custom_MetaData_Delete]", connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;

		connection.Open();
		command.ExecuteNonQuery();
		connection.Close();
	}

	public static void Delete(Guid contentId, string key)
	{
		using var connection = GetSqlConnection();
		using var command = CreateSprocCommand("[custom_MetaData_Delete_Key]", connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;
		command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;

		connection.Open();
		command.ExecuteNonQuery();
		connection.Close();
	}

	public static ContentMetadata Set(Guid contentId, Guid contentTypeId, string key, string value)
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
				using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
				{
					if (reader.Read())
					{
						return Populate(reader);
					}
				}
				connection.Close();
			}
		}

		return new ContentMetadata();
	}

	private static ContentMetadata Populate(IDataReader reader)
	{
		var article = new ContentMetadata(
			reader["ContentId"] == null ? Guid.Empty : new Guid(reader["ContentId"].ToString()),
			reader["ContentTypeId"] == null ? Guid.Empty : new Guid(reader["ContentTypeId"].ToString()),
			reader["DataKey"].ToString(),
			reader["DataValue"].ToString()
		);

		return article;
	}
}