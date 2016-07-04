using System;
using System.Reflection;
using MetaContent.Data;
using Telligent.DynamicConfiguration.Components;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Version1;

namespace MetaContent.Plugins
{
    public class MetaContentPlugin : IInstallablePlugin, IRequiredConfigurationPlugin
    {
        private IPluginConfiguration Configuration { get; set; }

        #region IPlugin Members

        public string Name => "MetaContent Plugin";

        public string Description => "Adds support to for generic metadata to be added to IContent Entities.";

        public void Initialize()
        {
            PublicApi.Content.Events.AfterDelete += EventsOnAfterDelete;
        }

        private void EventsOnAfterDelete(ContentAfterDeleteEventArgs e)
        {
            // TODO: Consider moving to IEvolutionJob to avoid blocking UI
            Api.PublicApi.Delete(e.ContentId);
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

    }
}
