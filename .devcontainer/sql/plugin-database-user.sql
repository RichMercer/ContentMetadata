-- Development only. Creates the SQL login, user and default schema that the
-- ContentMetadata plugin connects with, matching the self-hosted setup Verint
-- documents for ISqlServerConnectedPlugin (one user per plugin, each with its
-- own default schema). Verint's hosted environment does this automatically.
-- Variables: APP_DB_NAME, PLUGIN_USER, PLUGIN_PASSWORD.

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(PLUGIN_USER)')
	CREATE LOGIN [$(PLUGIN_USER)] WITH PASSWORD = N'$(PLUGIN_PASSWORD)', CHECK_POLICY = OFF;
GO

USE [$(APP_DB_NAME)];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(PLUGIN_USER)')
	CREATE USER [$(PLUGIN_USER)] FOR LOGIN [$(PLUGIN_USER)] WITH DEFAULT_SCHEMA = [$(PLUGIN_USER)];
GO

IF SCHEMA_ID(N'$(PLUGIN_USER)') IS NULL
	EXEC (N'CREATE SCHEMA [$(PLUGIN_USER)] AUTHORIZATION [$(PLUGIN_USER)]');
GO

GRANT CREATE TABLE TO [$(PLUGIN_USER)];
GO
