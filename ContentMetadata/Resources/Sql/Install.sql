/***********************************************
*   Install new Tables and Stored Procedures   */
/***********************************************
* Table: custom_Metadata
***********************************************/
IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[custom_Metadata]') AND TYPE IN (N'U'))
BEGIN
	CREATE TABLE [dbo].[custom_Metadata](
		[ContentId] [uniqueidentifier] NOT NULL,
		[ContentTypeId] [uniqueidentifier] NOT NULL,
		[DataKey] [nvarchar](64) NOT NULL,
		[DataValue] [nvarchar](max) NOT NULL,
		CONSTRAINT [PK_custom_Metadata] PRIMARY KEY CLUSTERED 
		(
			[ContentId] ASC,
			[DataKey] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_Metadata_List]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_Metadata_List]
GO
CREATE PROCEDURE [dbo].[custom_Metadata_List]
		@ContentId		uniqueidentifier
AS
BEGIN
	SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM [custom_Metadata] WHERE [ContentId] = @ContentId
END
GO

GRANT EXECUTE ON [dbo].[custom_Metadata_List] TO PUBLIC
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_Metadata_Set]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_Metadata_Set]
GO
CREATE PROCEDURE [dbo].[custom_Metadata_Set]
		@ContentId		uniqueidentifier,
		@ContentTypeId	uniqueidentifier,
		@DataKey		nvarchar(64),
		@DataValue		nvarchar(max)
AS
BEGIN
	IF(LEN(@DataValue) > 0)
		IF((SELECT 1 FROM [custom_Metadata] WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey) > 0)
			UPDATE 
				[dbo].[custom_Metadata] SET [DataValue] = @DataValue
			WHERE
				[ContentId] = @ContentId AND
				[DataKey] = @DataKey
		ELSE
			INSERT INTO [dbo].[custom_Metadata]
				([ContentId]
				,[ContentTypeId]
				,[DataKey]
				,[DataValue])
			 VALUES (
				@ContentId,
				@ContentTypeId,
				@DataKey,
				@DataValue)
	ELSE
		DELETE FROM [custom_Metadata] WHERE ContentId = @ContentId AND DataKey = @DataKey

	SELECT [ContentId], [ContentTypeId], [DataKey], [DataValue] FROM [custom_Metadata] WHERE [ContentId] = @ContentId AND [DataKey] = @DataKey
END
GO

GRANT EXECUTE ON [dbo].[custom_Metadata_Set] TO PUBLIC
GO



IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_Metadata_Delete]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_Metadata_Delete]
GO
CREATE PROCEDURE [dbo].[custom_Metadata_Delete]
		@ContentId		uniqueidentifier
AS
BEGIN
	DELETE FROM [custom_Metadata] WHERE ContentId = @ContentId
END
GO

GRANT EXECUTE ON [dbo].[custom_Metadata_Delete] TO PUBLIC
GO


IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_Metadata_Delete_Key]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_Metadata_Delete_Key]
GO
CREATE PROCEDURE [dbo].[custom_Metadata_Delete_Key]
		@ContentId		uniqueidentifier,
		@DataKey		nvarchar(64)
AS
BEGIN
	DELETE FROM [custom_Metadata] WHERE ContentId = @ContentId AND DataKey = @DataKey
END
GO

GRANT EXECUTE ON [dbo].[custom_Metadata_Delete_Key] TO PUBLIC
GO