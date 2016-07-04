using System;
using System.Collections.Generic;
using System.Reflection;
using ContentMetadata.Api;
using ContentMetadata.Data;
using Telligent.DynamicConfiguration.Components;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Version1;
using PublicApi = Telligent.Evolution.Extensibility.Api.Version1.PublicApi;

namespace ContentMetadata.Plugins
{
    public class ContentMetadataPlugin : IInstallablePlugin, IRequiredConfigurationPlugin, IPluginGroup
    {
        private IPluginConfiguration Configuration { get; set; }

        #region IPlugin Members

        public string Name => "ContentMetadata Plugin";

        public string Description => "Adds support to for generic metadata to be added to IContent Entities.";

        public void Initialize()
        {
            PublicApi.Content.Events.AfterDelete += EventsOnAfterDelete;
        }

        private void EventsOnAfterDelete(ContentAfterDeleteEventArgs e)
        {
            // TODO: Consider moving to IEvolutionJob to avoid blocking UI
            Api.PublicApi.Instance.Delete(e.ContentId);
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

        public PropertyGroup[] ConfigurationOptions
        {
            get
            {
                var groups = new[] { new PropertyGroup("options", "Options", 0) };
                
                return groups;
            }
        }

        public void Update(IPluginConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region IPluginGroup Members

        public IEnumerable<Type> Plugins
        {
            get
            {
                return new[]
                {
                    typeof (WidgetApi)
                };
            }
        }

        #endregion

    }
}
