using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ContentMetadata.Data;

/// <summary>
/// Stores metadata in the plugin's own SQL Server schema. Verint Community 14 supplies
/// the connection string and schema through <see cref="Telligent.Evolution.Extensibility.Version1.ISqlServerConnectedPlugin"/>;
/// that connection only has access to the plugin schema.
/// </summary>
internal static class DataService
{
	private const string LegacyTable = "custom_Metadata";

	internal static readonly string[] LegacyProcedures =
	[
		"custom_Metadata_List",
		"custom_Metadata_List_Key",
		"custom_Metadata_Set",
		"custom_Metadata_Delete",
		"custom_Metadata_Delete_Key"
	];

	internal static string LegacyTableName => LegacyTable;

	private static volatile string _connectionString;
	private static volatile string _schema;

	#region Configuration

	internal static void Configure(string connectionString, string schema)
	{
		_connectionString = connectionString;
		_schema = string.IsNullOrWhiteSpace(schema) ? null : schema;
	}

	internal static bool IsConfigured => !string.IsNullOrEmpty(_connectionString);

	private static SqlConnection GetSqlConnection()
	{
		if (!IsConfigured)
			throw new InvalidOperationException("ContentMetadata has not been given a SQL Server connection by the platform. Ensure the ContentMetadata Plugin is enabled and, on self-hosted sites, that it has an entry in PluginConnectionStrings.");

		return new SqlConnection(_connectionString);
	}

	/// <summary>
	/// The plugin schema, falling back to the connection's default schema if the platform did not provide one.
	/// </summary>
	private static string Schema
	{
		get
		{
			if (_schema != null)
				return _schema;

			using var connection = GetSqlConnection();
			using var command = new SqlCommand("SELECT SCHEMA_NAME()", connection);
			connection.Open();
			_schema = (string)command.ExecuteScalar();
			return _schema;
		}
	}

	private static string QuotedSchema => Schema.Replace("]", "]]");

	private static string Table => $"[{QuotedSchema}].[Metadata]";

	#endregion

	#region Install and migration

	internal static void Install()
	{
		ExecuteScript("Install.sql");
	}

	/// <summary>
	/// True when the pre-v14 dbo table exists and is visible to the plugin connection.
	/// </summary>
	internal static bool LegacyTableExists()
	{
		using var connection = GetSqlConnection();
		using var command = new SqlCommand($"SELECT CASE WHEN OBJECT_ID(N'[dbo].[{LegacyTable}]', N'U') IS NULL THEN 0 ELSE 1 END", connection);
		connection.Open();
		return (int)command.ExecuteScalar() == 1;
	}

	/// <summary>
	/// Copies legacy dbo rows into the plugin schema.
	/// </summary>
	/// <returns>The number of legacy rows still missing from the plugin schema; 0 means everything was copied.</returns>
	internal static int MigrateLegacyData()
	{
		var remaining = 0;
		using var connection = GetSqlConnection();
		connection.Open();
		using var transaction = connection.BeginTransaction();

		foreach (var statement in GetBatchedStatements("MigrateLegacyData.sql"))
		{
			using var command = new SqlCommand(statement, connection, transaction);
			remaining = Convert.ToInt32(command.ExecuteScalar());
		}

		transaction.Commit();
		return remaining;
	}

	private static void ExecuteScript(string resourceName)
	{
		using var connection = GetSqlConnection();
		connection.Open();
		foreach (var statement in GetBatchedStatements(resourceName))
		{
			using var command = new SqlCommand(statement, connection);
			command.ExecuteNonQuery();
		}
	}

	private static IEnumerable<string> GetBatchedStatements(string resourceName)
	{
		var script = ReadResource(resourceName).Replace("{Schema}", QuotedSchema);
		return Regex.Split(script, @"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)
			.Select(statement => statement.Trim())
			.Where(statement => statement.Length > 0);
	}

	private static string ReadResource(string resourceName)
	{
		using var stream = typeof(DataService).Assembly.GetManifestResourceStream($"ContentMetadata.Resources.Sql.{resourceName}")
			?? throw new InvalidOperationException($"Embedded SQL resource {resourceName} was not found.");
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	#endregion

	public static List<ContentMetadata> List(Guid contentId)
	{
		using var connection = GetSqlConnection();
		using var command = new SqlCommand($"SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM {Table} WHERE [ContentId] = @ContentId", connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;

		return ReadAll(connection, command);
	}

	public static List<ContentMetadata> ListContent(string key, string value)
	{
		using var connection = GetSqlConnection();
		using var command = new SqlCommand($"SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM {Table} WHERE [DataKey] = @DataKey AND [DataValue] = @DataValue", connection);
		command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;
		command.Parameters.Add("@DataValue", SqlDbType.NVarChar, -1).Value = value;

		return ReadAll(connection, command);
	}

	public static void Delete(Guid contentId)
	{
		using var connection = GetSqlConnection();
		using var command = new SqlCommand($"DELETE FROM {Table} WHERE [ContentId] = @ContentId", connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;

		connection.Open();
		command.ExecuteNonQuery();
	}

	public static void Delete(Guid contentId, string key)
	{
		using var connection = GetSqlConnection();
		using var command = new SqlCommand($"DELETE FROM {Table} WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey", connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;
		command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;

		connection.Open();
		command.ExecuteNonQuery();
	}

	/// <summary>
	/// Inserts or updates a value. An empty value deletes the key, matching the behaviour of the
	/// pre-v14 custom_Metadata_Set procedure.
	/// </summary>
	public static ContentMetadata Set(Guid contentId, Guid contentTypeId, string key, string value)
	{
		var sql = $@"
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF LEN(@DataValue) > 0
BEGIN
	UPDATE {Table} WITH (UPDLOCK, SERIALIZABLE) SET [DataValue] = @DataValue
	WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey;

	IF @@ROWCOUNT = 0
		INSERT INTO {Table} ([ContentId], [ContentTypeId], [DataKey], [DataValue])
		VALUES (@ContentId, @ContentTypeId, @DataKey, @DataValue);
END
ELSE
	DELETE FROM {Table} WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey;
COMMIT TRANSACTION;

SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM {Table} WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey;";

		using var connection = GetSqlConnection();
		using var command = new SqlCommand(sql, connection);
		command.Parameters.Add("@ContentId", SqlDbType.UniqueIdentifier).Value = contentId;
		command.Parameters.Add("@ContentTypeId", SqlDbType.UniqueIdentifier).Value = contentTypeId;
		command.Parameters.Add("@DataKey", SqlDbType.NVarChar, 64).Value = key;
		command.Parameters.Add("@DataValue", SqlDbType.NVarChar, -1).Value = (object)value ?? string.Empty;

		return ReadAll(connection, command).FirstOrDefault() ?? new ContentMetadata();
	}

	private static List<ContentMetadata> ReadAll(SqlConnection connection, SqlCommand command)
	{
		var items = new List<ContentMetadata>();
		connection.Open();
		using var reader = command.ExecuteReader();
		while (reader.Read())
			items.Add(Populate(reader));

		return items;
	}

	private static ContentMetadata Populate(IDataReader reader)
	{
		return new ContentMetadata(
			reader.GetGuid(0),
			reader.GetGuid(1),
			reader.GetString(2),
			reader.GetString(3)
		);
	}
}
