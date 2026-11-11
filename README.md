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

| Package version | Verint Community | .NET |
| --- | --- | --- |
| 14.x | 14 | .NET 10 |
| 1.x | 11 to 13 | .NET Framework 4.7.2 |

To access the APIs, enable the `ContentMetadata Plugin` in your Verint Community instance. On install, the plugin creates its `Metadata` table in its own SQL Server schema.

## Database access (Verint Community 14)

Verint Community 14 no longer lets plugins use the Community database directly. ContentMetadata implements `ISqlServerConnectedPlugin`, so the platform supplies a connection string that can only reach the plugin's own schema.

* **Verint hosted:** nothing to configure. The schema and connection string are created for you.
* **Self-hosted:** create a SQL Server login and user for the plugin, give the user its own default schema (owned by that user) and `CREATE TABLE` permission, then add the connection string to the `PluginConnectionStrings` section of your [environment configuration](https://community.telligent.com/community/14/a/user-documentation/UD2221/how-do-i-configure-environment-options):

  ```json
  "PluginConnectionStrings": {
    "ContentMetadata.Plugins.ContentMetadataPlugin, ContentMetadata": "server=...;database=...;uid=contentmetadata;pwd=...;TrustServerCertificate=True"
  }
  ```

  Without this entry, the platform won't initialise the plugin. [.devcontainer/sql/plugin-database-user.sql](.devcontainer/sql/plugin-database-user.sql) is a working example.

## Upgrading from 1.x

Earlier versions stored metadata in `[dbo].[custom_Metadata]` with stored procedures in `dbo`. The plugin also implements `IDboAccessSqlServerConnectedPlugin`, but only for migration; nothing reads or writes `dbo` at runtime. When the plugin installs, it:

1. Copy every row from `[dbo].[custom_Metadata]` into `[<plugin schema>].[Metadata]`, skipping rows that are already there.
2. Check that every legacy row is present in the plugin schema.
3. Remove the legacy table and the `custom_Metadata_*` procedures, but only if step 2 succeeded. If any row is missing, the legacy table is kept and an error is written to the event log (category `ContentMetadata`, event 3662).

The migration runs every time the plugin is installed, and it is safe to repeat. Back up the database before upgrading.

**Self-hosted sites need one extra step.** The platform-managed read access to `dbo` during install is only available in Verint's hosted environment. On a self-hosted site, the plugin connects with the login in `PluginConnectionStrings`, so before upgrading, grant that user read access to the legacy table:

```sql
GRANT SELECT ON [dbo].[custom_Metadata] TO [contentmetadata];
```

Once the data is migrated, the plugin drops the table, and the grant goes with it. If the table isn't visible during install, the plugin writes event 3664 to the event log and leaves `dbo` alone. Install only runs when the plugin version changes, so make the grant before you deploy 14.x.

# Usage

Each of the 3 API's use the same base implementation meaning you get a consisisten set of abilities. Each provides the ability to List, Get, Set and Delete metadata associated with an IContent entity. In all cases the ContentId of the content you are storing data against must be provided. 

## REST API scopes

Verint Community 14 only lets a caller use REST endpoints whose scopes it has been granted. The ContentMetadata endpoints (`metadata/{ContentId}`, `findmetadata/{Key}`, and POST and DELETE on `metadata`) belong to the **Content Metadata** scope group: `contentmetadata.metadata.readonly` for GET and the matching modify scope for POST and DELETE. Grant these to the API keys, OAuth clients or widgets that need them. The endpoints don't do per-content permission checks, so grant them only to trusted callers.

## Search

The plugin allows you to optionally add metadata into the search index. When an item is indexed and it has metadata stored against it, it will be added to solr using the item key in lowercase prefixed by metadata_. For example, if you add a value with a key `MyData`, it will be added to Solr with a field name of `metadata_mydata`. This allows you to use the out of the box search API's e.g. you could pass `query=metadata_mydata:myvalue` into the SearchResults API. Please be aware that the search field and value are case sensitive when using Solr.

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

# Development

The repository includes the Verint Community 14 local stack from the `community-plugin` template (`dotnet new install Verint.Community.Templates`, then `dotnet new community-plugin`). It runs web and job nodes, SQL Server, Solr, the infra cache and message bus, the Aspire telemetry dashboard and MailPit in Docker Compose. Build output in `artifacts/build` is mounted into the stack as `/app/cfs/plugins`.

## Local endpoints

| Service | URL |
| --- | --- |
| Community (admin / p) | http://localhost:5863 |
| Telemetry dashboard (Aspire) | http://localhost:5260 |
| MailPit | http://localhost:5397 |
| Solr | http://localhost:5361 |
| SQL Server (SA / T3ll1g3nt) | localhost,5994 |

These ports are 100 above the template defaults, so this stack can run alongside other Community projects.

## Getting started

1. Accept the SQL Server End-User License Agreement once: `bash scripts/accept-mssql-eula.sh` (or `pwsh scripts\accept-mssql-eula.ps1`, or the VS Code task). This writes `.devcontainer/mssql-eula.env`, which is git-ignored.
2. Start the stack in one of two ways:
   * **Dev container:** open the folder in VS Code and choose **Reopen in Container**. You don't need .NET 10 on the host.
   * **Native:** run `bash scripts/compose.sh up` and build on the host with .NET 10.
3. Build the plugin with `dotnet build ContentMetadata/ContentMetadata.csproj` (or the **Build** task), then restart Community with `bash scripts/compose.sh restart web job infra` (or the **Compose Restart** task).

`.devcontainer/settings.json` enables the plugin and supplies its `PluginConnectionStrings` entry. The one-shot `plugin-database` service creates the `contentmetadata` SQL login, user and schema it connects with.

## Testing the 1.x migration

Start the stack with `SEED_LEGACY_METADATA=true bash scripts/compose.sh up` on a fresh database. Before Community starts, the `plugin-database` service creates `[dbo].[custom_Metadata]` with three sample rows. When the plugin installs, those rows should appear in `[contentmetadata].[Metadata]` and the dbo table should be removed. To start over, run `bash scripts/compose.sh down --volumes`.

## Packaging

`dotnet pack ContentMetadata/ContentMetadata.csproj -c Release` writes the NuGet package to `artifacts/package`.

## Releasing

[.github/workflows/nuget.yml](.github/workflows/nuget.yml) builds and packs on every push and pull request. It publishes to nuget.org only when you push a `v14.x` tag, and the package version comes from the tag:

```sh
git tag v14.0.0
git push origin v14.0.0
```

Publishing uses [NuGet Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing), so there's no API key to store or rotate. It also runs in the protected `nuget` environment, which you approve before the push. The one-time setup is listed at the top of the workflow.
