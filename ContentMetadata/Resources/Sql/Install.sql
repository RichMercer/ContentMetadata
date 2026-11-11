/*
 * Creates the ContentMetadata table in the plugin's own schema. {Schema} is
 * replaced at install time with the schema Verint Community assigns the plugin.
 * Safe to run repeatedly.
 */
IF OBJECT_ID(N'[{Schema}].[Metadata]', N'U') IS NULL
BEGIN
	CREATE TABLE [{Schema}].[Metadata](
		[ContentId] [uniqueidentifier] NOT NULL,
		[ContentTypeId] [uniqueidentifier] NOT NULL,
		[DataKey] [nvarchar](64) NOT NULL,
		[DataValue] [nvarchar](max) NOT NULL,
		CONSTRAINT [PK_Metadata] PRIMARY KEY CLUSTERED ([ContentId] ASC, [DataKey] ASC)
	)
END
GO
