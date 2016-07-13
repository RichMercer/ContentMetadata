# TelligentContentMetadata
ContentMetadata for Telligent Community adds support for Metadata on Telligent Community IContent entities. It was built as a replacement for ExtendedAttributes for developers who were using them to store their own custom data.
Although ExtendedAttributes have proved useful, they have many limitations. 
* They are not available on all entities (e.g. wikis, comments etc.).
* When too much data is stored against an object performance can be affected.
* Lookup based on ExtendedAttributes is not possible due to lack of API filtering.

This project seeks to solve these problem by saving metadata in its own table and exposes a full InProcess API, Widget API via Velocity extensions and a RESTful API. The plugin uses a custom SQL table to store the custom metadata which will allow for more functionality to be added in the future.

# Installation
To add ContentMetadata to your project, you can add it via nuget.

* `Install-Package TelligentContentMetadata`

To access the API's, you will need to enable the plugin `ContentMetadata Plugin` in your Telligent Community instance. If your sites' connection string has dbowner access to your database, the install script will be run automatically when the plugin is enabled. If not, or you would prefer to run the script yourself, you can execute the SQL file here.

* [SQL Install Script](https://raw.githubusercontent.com/RichMercer/ContentMetadata/master/ContentMetadata/Resources/Sql/Install.sql)

# Usage

Each of the 3 API's use the same base implementation meaning you get a consisisten set of abilities. Each provides the ability to List, Get, Set and Delete metadata associated with an IContent entity.

## InProcess API
The InProcess API can be used to set and access metadata using C# in plugins or other custom code.

### List

`var metadata = Apis.Get<IContentMetadataApi>().List(contentId);`

### Get

`var metadata = Apis.Get<IContentMetadataApi>().Get(contentId, key);`

### Set

`var metadata = Apis.Get<IContentMetadataApi>().Set(contentId, contentTypeId, key, value);`

### Delete

`Apis.Get<IContentMetadataApi>().Delete(contentId);` - Deletes all metadata for a piece of content.

or

`Apis.Get<IContentMetadataApi>().Delete(contentId, key);` - Delete a specific piece of metadata based on the key.

## Widget API

The widget API exposes the same commands as above using Velocity extensions.

### List

`#set($response = $metadata_v1_content.List($contentId))`

### Get

`#set($response = $metadata_v1_content.Get($contentId, $key))`

### Set

`#set($reponse = $metadata_v1_content.Set($contentId, $contentTypeId, $key, $value))`

### Delete

`$metadata_v1_content.Delete($contentId)` - Deletes all metadata for a piece of content.

or

`$metadata_v1_content.Delete($contentId, $key)` - Delete a specific piece of metadata based on the key.
