using System;
using System.Collections.Generic;
using System.Reflection;
using ContentMetadata.Api;
using ContentMetadata.Data;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Version1;
using Property = Telligent.Evolution.Extensibility.Configuration.Version1.Property;
using PropertyGroup = Telligent.Evolution.Extensibility.Configuration.Version1.PropertyGroup;

namespace ContentMetadata.Plugins
{
    public class ContentMetadataPlugin : IInstallablePlugin, Telligent.Evolution.Extensibility.Version2.IConfigurablePlugin, IPluginGroup
    {
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
        
        #region IInstallablePlugin Members

        public Version Version => Assembly.GetAssembly(GetType()).GetName().Version;

        public void Install(Version lastInstalledVersion)
        {
            if (Version <= lastInstalledVersion) return;

            DataService.Install();
        }

        public void Uninstall()
        {
            // Nothing to be done to uninstall. Leave data intact.
        }

        #endregion

        #region IRequiredConfigurationPlugin Members

        public bool IsConfigured => DataService.IsInstalled();


        public void Update(Telligent.Evolution.Extensibility.Version2.IPluginConfiguration configuration)
        {
            Configuration = configuration;
        }

        public PropertyGroup[] ConfigurationOptions
        {
            get
            {
                PropertyGroup[] groups =
                {
                    new PropertyGroup
                    {
                        Id = "options",
                        LabelText = "Options",
                        OrderNumber = 0
                    }
                };

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
}
