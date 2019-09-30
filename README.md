# TelligentContentMetadata
ContentMetadata for Telligent Community adds support for Metadata on Telligent Community IContent entities. It acts as a store for Key/Value pairs of information and was built as a replacement for using ExtendedAttributes for developers who were using them to store their own custom data. Using the ContentId for an entity, Key/Value pairs can be stored linked to the content original content.

Although ExtendedAttributes have proved useful, they have many limitations. 
* They are not available on all entities (e.g. wikis, comments etc.).
* When too much data is stored against an object performance can be affected.
* Lookup based on ExtendedAttributes is not possible due to lack of API filtering.
* Extended attributes have no access contorl - anyone who can read the underlying entity can read all extended attributes, and anyone who can modify the underlying entity can modify extended attributes.  This means that you can't use extended attributes for storing anything that is security sensitive, as well as making it difficult to restrict allowed values.

This project seeks to solve these problem by saving metadata in its own table and exposes a full InProcess API, Widget API via Velocity extensions and a RESTful API. The plugin uses a custom SQL table to store the custom metadata which will allow for more functionality to be added in the future.

# Installation
To add ContentMetadata to your project, you can add it via nuget.

```powershell
Install-Package TelligentContentMetadata
```
To access the API's, you will need to enable the plugin `ContentMetadata Plugin` in your Telligent Community instance. If your sites' connection string has dbowner access to your database, the install script will be run automatically when the plugin is enabled. If not, or you would prefer to run the script yourself, you can execute the SQL file here.

* [SQL Install Script](https://raw.githubusercontent.com/RichMercer/ContentMetadata/master/ContentMetadata/Resources/Sql/Install.sql)

# Usage

Each of the 3 API's use the same base implementation meaning you get a consisisten set of abilities. Each provides the ability to List, Get, Set and Delete metadata associated with an IContent entity. In all cases the ContentId of the content you are storing data against must be provided. Currently there is no REST API due to concerns around information disclosure. I am looking at ways to secure the access to ContentMetada calls inline with the platforms own checks on permissions.

## Search

The plugin allows you to optionally add metadata into the search index. When an item is indexed and it has metadata stored against it, it will be added to solr using the item key in lowercase prefixed by metadata_. For example, if you add a value with a key `MyData`, it will be added to Solr with a field name of `metadata_mydata`. This allows you to use the out of the box search API's e.g. you could pass `query=metadata_mydata:myvalue` into the SearchResults API.

## InProcess API
The InProcess API can be used to set and access metadata using C# in plugins or other custom code.

### List

```cs
var metadata = Apis.Get<IContentMetadataApi>().List(contentId);
```

### Get

```cs
var metadata = Apis.Get<IContentMetadataApi>().Get(contentId, key);
```

### Set

```cs
var metadata = Apis.Get<IContentMetadataApi>().Set(contentId, contentTypeId, key, value);
```

### Delete

```cs
Apis.Get<IContentMetadataApi>().Delete(contentId); //Deletes all metadata for a piece of content.
```

or

```cs
Apis.Get<IContentMetadataApi>().Delete(contentId, key); //Delete a specific piece of metadata based on the key.
```

## Widget API

The widget API exposes the same commands as above using Velocity extensions.

### List

```velocity
#set($response = $metadata_v1_content.List($contentId))
```

### Get

```velocity
#set($response = $metadata_v1_content.Get($contentId, $key))
```

### Set

```velocity
#set($reponse = $metadata_v1_content.Set($contentId, $contentTypeId, $key, $value))
```

### Delete

```velocity
$metadata_v1_content.Delete($contentId) #Deletes all metadata for a piece of content.
```
or
```velocity
$metadata_v1_content.Delete($contentId, $key) #Delete a specific piece of metadata based on the key.
```

## Supported Entities
Below is a list of entities that implement IContent. The list shows commonly used classes but isn't exhaustive.
* Achievement
* Blog
* BlogPost
* Calendar
* Event
* KnowledgeCollection
* Document
* Challenge
* Idea
* Comment
* Content
* ContentPage
* Conversation
* ConversationMessage
* ExternalMessage
* Forum
* ForumReply
* ForumThread
* Gallery
* Media
* Group
* GroupUser
* Leaderboard
* StatusMessage
* Tweet
* User
* Wiki
* WikiPage
