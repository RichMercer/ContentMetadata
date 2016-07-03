using System;
using System.Reflection;
using Telligent.Evolution.Extensibility.Version1;

namespace MetaContent
{
    public class MetaContentPlugin : IInstallablePlugin
    {
        #region IPlugin Members

        public string Name => "MetaContent Plugin";

        public string Description => "Adds support to for generic metadata to be added to IContent Entities.";

        public void Initialize()
        {
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

    }
}
