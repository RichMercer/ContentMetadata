/***********************************************
*   Install new Tables and Stored Procedures   */
/***********************************************
* Table: te_SharePoint_List
***********************************************/
IF NOT EXISTS (SELECT * FROM SYS.OBJECTS WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[custom_MetaData]') AND TYPE IN (N'U'))
BEGIN
	CREATE TABLE [dbo].[custom_MetaData]
	(
		[ContentId]		uniqueidentifier NOT NULL,
		[ContentTypeId] uniqueidentifier NOT NULL,
		[DataKey]		nvarchar(64) NOT NULL,
		[DataValue]		nvarchar(max) NOT NULL,
		[DataType]		nvarchar(64) NOT NULL
	)
END
GO


IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custom_MetaData_Update]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[custom_MetaData_Update]
GO
CREATE PROCEDURE [dbo].[custom_MetaData_Update]
		@ContentId		uniqueidentifier,
		@ContentTypeId	uniqueidentifier,
		@DataKey		nvarchar(64),
		@DataValue		nvarchar(max),
		@DataType		nvarchar(64)
AS
BEGIN
	INSERT INTO [dbo].[custom_MetaData]
           ([ContentId]
           ,[ContentTypeId]
           ,[DataKey]
           ,[DataValue]
           ,[DataType])
     VALUES (
			@ContentId,
			@ContentTypeId,
			@DataKey,
			@DataValue,
			@DataType)
END
GO

GRANT EXECUTE ON [dbo].[custom_MetaData_Update] TO PUBLIC
GO


