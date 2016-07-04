USE [metacontent.local]
GO
/***********************************************
*   Install new Tables and Stored Procedures   */
/***********************************************
* Table: custom_MetaData
***********************************************/
IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[custom_MetaData]') AND TYPE IN (N'U'))
BEGIN
	CREATE TABLE [dbo].[custom_MetaData](
		[ContentId] [uniqueidentifier] NOT NULL,
		[ContentTypeId] [uniqueidentifier] NOT NULL,
		[DataKey] [nvarchar](64) NOT NULL,
		[DataValue] [nvarchar](max) NOT NULL,
		CONSTRAINT [PK_custom_MetaData] PRIMARY KEY CLUSTERED 
		(
			[ContentId] ASC,
			[DataKey] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	CREATE NONCLUSTERED INDEX [IX_custom_MetaData] ON [dbo].[custom_MetaData]
	(
		[ContentId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_MetaData_Get]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_MetaData_Get]
GO
CREATE PROCEDURE [dbo].[custom_MetaData_Get]
		@ContentId		uniqueidentifier,
		@DataKey		nvarchar(64)
AS
BEGIN
	SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM [custom_MetaData] WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey
END
GO

GRANT EXECUTE ON [dbo].[custom_MetaData_Get] TO PUBLIC
GO


IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_MetaData_Set]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_MetaData_Set]
GO
CREATE PROCEDURE [dbo].[custom_MetaData_Set]
		@ContentId		uniqueidentifier,
		@ContentTypeId	uniqueidentifier,
		@DataKey		nvarchar(64),
		@DataValue		nvarchar(max)
AS
BEGIN
	IF((SELECT 1 FROM [custom_MetaData] WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey) > 0)
		UPDATE 
			[dbo].[custom_MetaData] SET [DataValue] = @DataValue
		WHERE
			[ContentId] = @ContentId AND
			[DataKey] = @DataKey
	ELSE
		INSERT INTO [dbo].[custom_MetaData]
			([ContentId]
			,[ContentTypeId]
			,[DataKey]
			,[DataValue])
		 VALUES (
			@ContentId,
			@ContentTypeId,
			@DataKey,
			@DataValue)
END
GO

GRANT EXECUTE ON [dbo].[custom_MetaData_Set] TO PUBLIC
GO
