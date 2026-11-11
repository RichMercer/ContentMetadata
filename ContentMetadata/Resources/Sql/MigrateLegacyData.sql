/*
 * Copies metadata from the pre-v14 [dbo].[custom_Metadata] table into the
 * plugin schema. Rows already present in the plugin schema are left untouched,
 * so it is safe to run repeatedly. Returns the number of legacy rows that are
 * still missing from the plugin schema afterwards (0 means fully migrated).
 */
INSERT INTO [{Schema}].[Metadata] ([ContentId], [ContentTypeId], [DataKey], [DataValue])
SELECT l.[ContentId], l.[ContentTypeId], l.[DataKey], l.[DataValue]
FROM [dbo].[custom_Metadata] l
WHERE NOT EXISTS (
	SELECT 1 FROM [{Schema}].[Metadata] m
	WHERE m.[ContentId] = l.[ContentId] AND m.[DataKey] = l.[DataKey]
)

SELECT COUNT(*)
FROM [dbo].[custom_Metadata] l
WHERE NOT EXISTS (
	SELECT 1 FROM [{Schema}].[Metadata] m
	WHERE m.[ContentId] = l.[ContentId] AND m.[DataKey] = l.[DataKey]
)
GO
