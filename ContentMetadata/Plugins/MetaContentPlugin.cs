using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ContentMetadata.Api;
using ContentMetadata.Data;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Version1;
using Property = Telligent.Evolution.Extensibility.Configuration.Version1.Property;
using PropertyGroup = Telligent.Evolution.Extensibility.Configuration.Version1.PropertyGroup;

namespace ContentMetadata.Plugins;

/// <summary>
/// Stores metadata in the plugin schema provided through <see cref="ISqlServerConnectedPlugin"/>.
/// <see cref="IDboAccessSqlServerConnectedPlugin"/> is implemented only so that sites upgrading from
/// versions before 14 can have their [dbo].[custom_Metadata] rows migrated into the plugin schema
/// during installation; nothing reads or writes dbo at runtime.
/// </summary>
public class ContentMetadataPlugin : IDboAccessSqlServerConnectedPlugin, Telligent.Evolution.Extensibility.Version2.IConfigurablePlugin, IPluginGroup
{
    private const string EventLogCategory = "ContentMetadata";

    private IDboAccessSqlServerConnectedPluginController _dboController;

    private Telligent.Evolution.Extensibility.Version2.IPluginConfiguration Configuration { get; set; }

    public bool EnableSearch => Configuration.GetBool("enableSearch").GetValueOrDefault(false);

    #region IPlugin Members

    public string Name => "ContentMetadata Plugin";

    public string Description => "Adds support to for generic metadata to be added to IContent Entities.";

    public void Initialize()
    {
        Apis.Get<IContents>().Events.AfterDelete += EventsOnAfterDelete;
        Apis.Get<ISearchIndexing>().Events.BeforeBulkIndex += EventsOnBeforeBulkIndex;
    }

    private void EventsOnBeforeBulkIndex(BeforeBulkIndexingEventArgs e)
    {
        if (!EnableSearch)
            return;

        foreach (var document in e.Documents)
        {
            var metadata = Apis.Get<IContentMetadataApi>().List(document.ContentId);

            foreach (var item in metadata)
            {
                document.AddField("metadata_" + item.Key.ToLower(), item.Value);
            }
        }
    }

    private void EventsOnAfterDelete(ContentAfterDeleteEventArgs e)
    {
        // TODO: Consider moving to IEvolutionJob
        Apis.Get<IContentMetadataApi>().Delete(e.ContentId);
    }

    #endregion

    #region ISqlServerConnectedPlugin Members

    public void SetController(ISqlServerConnectedPluginController controller)
    {
        DataService.Configure(controller.ConnectionString, controller.Schema);
    }

    public void SetController(IDboAccessSqlServerConnectedPluginController controller)
    {
        _dboController = controller;
    }

    #endregion

    #region IInstallablePlugin Members

    public Version Version => Assembly.GetAssembly(GetType()).GetName().Version;

    public void Install(Version lastInstalledVersion)
    {
        if (!DataService.IsConfigured)
        {
            WriteLog("ContentMetadata could not be installed because no SQL Server connection was provided. On self-hosted sites, add the plugin to PluginConnectionStrings.", "Error", 3661);
            return;
        }

        // Creating the table is idempotent, so always run it: the plugin schema may have been
        // removed (Administration > Site > Extension Data) since the last install.
        DataService.Install();

        MigrateLegacyData();
    }

    public Task InstallAsync(Version lastInstalledVersion, CancellationToken cancellationToken)
    {
        Install(lastInstalledVersion);
        return Task.CompletedTask;
    }

    public void Uninstall()
    {
        // Leave data intact. Verint Community manages the plugin schema.
    }

    public Task UninstallAsync(CancellationToken cancellationToken)
    {
        Uninstall();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Copies rows from the pre-v14 [dbo].[custom_Metadata] table into the plugin schema, then removes
    /// the legacy table and procedures once every row is confirmed to be present in the plugin schema.
    /// </summary>
    private void MigrateLegacyData()
    {
        if (_dboController == null)
            return;

        if (!DataService.LegacyTableExists())
        {
            WriteLog($"ContentMetadata found no legacy [dbo].[{DataService.LegacyTableName}] table to migrate. If this site is upgrading from ContentMetadata 1.x and is self-hosted, grant the plugin's SQL user SELECT on that table before upgrading so its data can be migrated.", "Information", 3664);
            return;
        }

        var remaining = DataService.MigrateLegacyData();
        if (remaining > 0)
        {
            WriteLog($"ContentMetadata copied legacy metadata from [dbo].[{DataService.LegacyTableName}] but {remaining} row(s) could not be verified in the plugin schema. The legacy table has been kept.", "Error", 3662);
            return;
        }

        foreach (var procedure in DataService.LegacyProcedures)
            _dboController.TryDeleteProcedure(procedure);

        _dboController.TryDeleteTable(DataService.LegacyTableName);

        WriteLog($"ContentMetadata migrated legacy metadata from [dbo].[{DataService.LegacyTableName}] into the plugin schema and removed the legacy table and procedures.", "Information", 3663);
    }

    private static void WriteLog(string message, string eventType, int eventId)
    {
        Apis.Get<IEventLog>().Write(message, new EventLogEntryWriteOptions { EventType = eventType, Category = EventLogCategory, EventId = eventId });
    }

    #endregion

    #region IConfigurablePlugin Members

    public void Update(Telligent.Evolution.Extensibility.Version2.IPluginConfiguration configuration)
    {
        Configuration = configuration;
    }

    public PropertyGroup[] ConfigurationOptions
    {
        get
        {
            PropertyGroup[] groups =
            [
                new PropertyGroup
                {
                    Id = "options",
                    LabelText = "Options",
                    OrderNumber = 0
                }
            ];

            var enableSearch = new Property
            {
                Id = "enableSearch",
                LabelText = "Enable search",
                DescriptionText = "When enabled, metadata stored against an object will be added to the search index.",
                DataType = "Bool",
                OrderNumber = 0,
                DefaultValue = "false"
            };

            groups[0].Properties.Add(enableSearch);

            return groups;
        }
    }

    #endregion

    #region IPluginGroup Members

    public IEnumerable<Type> Plugins => new[]
    {
        typeof (WidgetApi),
        typeof (RestApi),
        typeof (InProcessApi)
    };

    #endregion

}
