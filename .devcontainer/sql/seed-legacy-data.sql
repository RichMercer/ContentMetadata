-- Development only. Recreates the pre-v14 dbo.custom_Metadata table with sample
-- rows so the migration into the plugin schema can be tested. Runs only when
-- SEED_LEGACY_METADATA=true and never overwrites an existing table.
-- It also grants the plugin user read access to the legacy table, which is the
-- step a self-hosted operator performs before upgrading from 1.x.
-- Variables: APP_DB_NAME, PLUGIN_USER.

USE [$(APP_DB_NAME)];
GO

IF OBJECT_ID(N'[dbo].[custom_Metadata]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[custom_Metadata](
		[ContentId] [uniqueidentifier] NOT NULL,
		[ContentTypeId] [uniqueidentifier] NOT NULL,
		[DataKey] [nvarchar](64) NOT NULL,
		[DataValue] [nvarchar](max) NOT NULL,
		CONSTRAINT [PK_custom_Metadata1] PRIMARY KEY CLUSTERED ([ContentId] ASC, [DataKey] ASC)
	);

	INSERT INTO [dbo].[custom_Metadata] ([ContentId], [ContentTypeId], [DataKey], [DataValue]) VALUES
		('11111111-1111-1111-1111-111111111111', 'f586769b-0822-468a-b7f3-a94d480ed9b0', N'Colour', N'Blue'),
		('11111111-1111-1111-1111-111111111111', 'f586769b-0822-468a-b7f3-a94d480ed9b0', N'Featured', N'true'),
		('22222222-2222-2222-2222-222222222222', 'f586769b-0822-468a-b7f3-a94d480ed9b0', N'Colour', N'Red');
END
GO

IF OBJECT_ID(N'[dbo].[custom_Metadata_List]', N'P') IS NULL
	EXEC (N'CREATE PROCEDURE [dbo].[custom_Metadata_List] @ContentId uniqueidentifier AS SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM [dbo].[custom_Metadata] WHERE [ContentId] = @ContentId');
GO

IF OBJECT_ID(N'[dbo].[custom_Metadata]', N'U') IS NOT NULL
	GRANT SELECT ON [dbo].[custom_Metadata] TO [$(PLUGIN_USER)];
GO
