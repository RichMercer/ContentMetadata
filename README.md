# ContentMetadata for Verint Community

ContentMetadata stores key/value metadata against any Verint Community (formerly Telligent Community) `IContent` entity: blog posts, forum threads, wiki pages, users, groups, comments and more. It was built to replace extended attributes for developers who used them to store their own data.

Full documentation: **https://richmercer.github.io/ContentMetadata/**

Extended attributes have several limitations that ContentMetadata avoids:

* They aren't available on every entity (for example wikis and comments).
* Storing a lot of data against an object can hurt performance.
* You can't look content up by an extended attribute value, because the APIs don't filter on them.
* Anyone who can read the entity can read all of its extended attributes, and anyone who can edit it can change them.

ContentMetadata keeps its data in its own table, separate from the entity, and you choose which APIs can reach it. It has an in-process API for plugins, a Velocity widget extension and a REST API. All three share one implementation. ContentMetadata does not check permissions on the content itself, so read [Security](#security) before exposing it to users.

# Installation

Add the package from NuGet:

```powershell
dotnet add package TelligentContentMetadata
```

| Package version | Verint Community | .NET |
| --- | --- | --- |
| 14.x | 14 | .NET 10 |
| 1.x | 11 to 13 | .NET Framework 4.7.2 |

Deploy `ContentMetadata.dll` with your other plugins, then enable **ContentMetadata Plugin** in Administration. On install, the plugin creates its `Metadata` table in its own SQL Server schema.

## Configuration

| Option | Default | What it does |
| --- | --- | --- |
| Enable search | Off | Adds each item's metadata to the search index. See [Search](#search). |

## Database access (Verint Community 14)

Verint Community 14 no longer lets plugins use the Community database directly. ContentMetadata implements `ISqlServerConnectedPlugin`, so the platform supplies a connection string that can only reach the plugin's own schema.

* **Verint hosted:** nothing to configure. The schema and connection string are created for you.
* **Self-hosted:** create a SQL Server login and user for the plugin, give the user its own default schema (owned by that user) and `CREATE TABLE` permission, then add the connection string to the `PluginConnectionStrings` section of your [environment configuration](https://community.telligent.com/community/14/a/user-documentation/UD2221/how-do-i-configure-environment-options):

  ```json
  "PluginConnectionStrings": {
    "ContentMetadata.Plugins.ContentMetadataPlugin, ContentMetadata": "server=...;database=...;uid=contentmetadata;pwd=...;TrustServerCertificate=True"
  }
  ```

  Without this entry, the platform won't initialise the plugin. [.devcontainer/sql/plugin-database-user.sql](.devcontainer/sql/plugin-database-user.sql) is a working example. Use a strong, unique password; the example's password is for the local stack only.

## Upgrading from 1.x

Earlier versions stored metadata in `[dbo].[custom_Metadata]` with stored procedures in `dbo`. The plugin also implements `IDboAccessSqlServerConnectedPlugin`, but only for migration; nothing reads or writes `dbo` at runtime. When the plugin installs, it:

1. Copies every row from `[dbo].[custom_Metadata]` into `[<plugin schema>].[Metadata]`, skipping rows that are already there.
2. Checks that every legacy row is present in the plugin schema.
3. Removes the legacy table and the `custom_Metadata_*` procedures, but only if step 2 succeeded. If any row is missing, the legacy table is kept and an error is written to the event log (event 3662).

The migration runs every time the plugin is installed, and it is safe to repeat. Back up the database before upgrading.

**Self-hosted sites need one extra step.** The platform-managed read access to `dbo` during install is only available in Verint's hosted environment. On a self-hosted site, the plugin connects with the login in `PluginConnectionStrings`, so before upgrading, grant that user read access to the legacy table:

```sql
GRANT SELECT ON [dbo].[custom_Metadata] TO [contentmetadata];
```

Once the data is migrated, the plugin drops the table, and the grant goes with it. If the table isn't visible during install, the plugin writes event 3664 to the event log and leaves `dbo` alone. Install only runs when the plugin version changes, so make the grant before you deploy 14.x.

## Event log

The plugin writes to the Community event log with the category `ContentMetadata`.

| Event | Type | Meaning |
| --- | --- | --- |
| 3661 | Error | No SQL Server connection was provided, so the table wasn't created. Add the `PluginConnectionStrings` entry. |
| 3662 | Error | Legacy rows were copied but some couldn't be verified. The legacy table was kept. |
| 3663 | Information | Legacy data was migrated and the legacy table and procedures were removed. |
| 3664 | Information | No legacy table was visible during install. |
| 3665 | Error | A REST request failed unexpectedly. The caller gets a generic error; the details are in this entry. |

# Usage

Every API offers the same operations: list, get, set and delete metadata for an `IContent` entity, plus a lookup of content by key and value. You always pass the `ContentId` of the content the metadata belongs to.

Rules that apply everywhere:

* Keys are 1 to 64 characters. A longer key is rejected with an `ArgumentException`.
* Keys are compared exactly, so `Colour` and `colour` are different keys.
* Values are strings of any length. Setting an empty value deletes the key.
* When content is deleted, its metadata is deleted too.
* Reads are cached for 100 seconds per content item, and a set or delete clears that cache on the server that made the change. Other web or job servers may serve the old value until their cache expires.

## InProcess API

Use `IContentMetadataApi` from C# in plugins or other custom code.

```cs
var api = Apis.Get<IContentMetadataApi>();

// All metadata for a piece of content
IReadOnlyList<ContentMetadata> items = api.List(contentId);

// One value. Returns an empty ContentMetadata (Value is null) when the key isn't set.
ContentMetadata item = api.Get(contentId, "MyKey");
bool featured = item.GetBoolValue(false);
int priority = item.GetIntValue(0);

// Add or update a value
api.Set(contentId, contentTypeId, "MyKey", "MyValue");

// Delete one key, or every key for the content
api.Delete(contentId, "MyKey");
api.Delete(contentId);

// Find content by metadata: every item with MyKey = MyValue,
// or the first one of a given content type
IReadOnlyList<ContentMetadata> matches = api.List("MyKey", "MyValue");
ContentMetadata match = api.Get(contentTypeId, "MyKey", "MyValue");
```

### Extension methods

`ContentMetadata.Extensions` adds helpers to `IContent`:

```cs
using ContentMetadata.Extensions;

var all = post.GetMetadata();
string value = post.GetMetadata("MyKey");
int count = post.GetMetadataInt("Count", 0);
bool enabled = post.GetMetadataBool("Enabled", false);
post.SetMetadata("MyKey", "MyValue");
```

## Widget API

The `$metadata_v1_content` Velocity extension exposes the same methods to widgets.

```velocity
#set($items = $metadata_v1_content.List($contentId))
#set($item = $metadata_v1_content.Get($contentId, 'MyKey'))
#set($item = $metadata_v1_content.Set($contentId, $contentTypeId, 'MyKey', 'MyValue'))
$metadata_v1_content.Delete($contentId, 'MyKey')  ## Delete one key
$metadata_v1_content.Delete($contentId)           ## Delete every key for the content
#set($matches = $metadata_v1_content.List('MyKey', 'MyValue'))
```

Values are returned as stored. Encode them before output (for example with `$core_v2_encoding.HtmlEncode($item.Value)`), and check the accessing user's permissions before you call `Set` or `Delete` from a callback. See [Security](#security).

## REST API

Endpoints are under `/api.ashx/v2/`. Add `.json` or `.xml` to choose the response format. Each response has `Data` and `Errors`; a missing key returns `Content Metadata Not Found.` in `Errors`.

| Method | Endpoint | Parameters | Returns |
| --- | --- | --- | --- |
| GET | `metadata/{ContentId}` | `Key` (optional) | Every item for the content, or the one item for `Key` |
| GET | `findmetadata/{Key}` | `Value` (required), `ContentTypeId` (optional) | Every item with that key and value, or the first one of that content type |
| POST | `metadata` | `ContentId`, `ContentTypeId`, `Key`, `Value` (all required) | The saved item |
| DELETE | `metadata` | `ContentId`, `Key` (both required) | Nothing |

Parameters can be sent in the query string or, for POST and DELETE, as form fields.

```sh
curl -H "Rest-User-Token: <token>" \
  "https://community.example.com/api.ashx/v2/metadata/3f2b...e1.json?Key=MyKey"
```

### Scopes

Verint Community 14 only lets a caller use REST endpoints whose scopes it has been granted. These endpoints belong to the **Content Metadata** scope group: `contentmetadata.metadata.readonly` for GET and the matching modify scope for POST and DELETE. Grant them only to the API keys, OAuth clients and widgets that need them.

## Search

When **Enable search** is on, each item's metadata is added to the search index as it is indexed. The field name is the key in lowercase with a `metadata_` prefix, so a key of `MyData` becomes `metadata_mydata`. You can then use the standard search APIs, for example `query=metadata_mydata:myvalue` in the SearchResults API. Solr field names and values are case sensitive.

Use keys made of letters, digits and underscores if you enable search; other characters can make the field name hard or impossible to query. Don't enable search if any metadata is sensitive.

## Security

ContentMetadata stores and returns whatever it is given. It does not check whether the caller can see or edit the content that a `ContentId` points to. Design your integration with that in mind:

* **REST:** anyone with the read scope can read metadata for any content ID, including content in private groups, and `findmetadata` can reveal which content IDs hold a value. Anyone with the modify scope can change or delete metadata on any content. Only grant these scopes to trusted callers, and don't store anything sensitive if untrusted users can reach the endpoints.
* **Widgets and plugins:** your code decides who can do what. Before you call `Set` or `Delete` with an ID from the request, check that the accessing user may edit that content, and HTML-encode values before rendering them.
* **Content type:** `ContentTypeId` is stored as supplied and isn't checked against the content, so don't rely on it alone for security decisions.
* **Search:** indexed metadata can be searched by anyone who can see the content.

Please report security issues privately through [GitHub security advisories](https://github.com/RichMercer/ContentMetadata/security/advisories/new) rather than in a public issue.

## Supported entities

Any entity that implements `IContent` is supported. Commonly used ones include:

Achievement, Blog, BlogPost, Calendar, Event, Challenge, Idea, Comment, ContentPage, Conversation, ConversationMessage, Forum, ForumThread, ForumReply, Gallery, Media, Group, GroupUser, KnowledgeCollection, Document, Leaderboard, StatusMessage, User, Wiki and WikiPage.

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

## Documentation site

[docs/index.html](docs/index.html) is the GitHub Pages site. It is one static HTML file with no build step. To publish it, open **Settings > Pages** in GitHub and set the source to **Deploy from a branch**, branch `main`, folder `/docs`. Keep it in step with this README when the API changes.
