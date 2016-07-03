using System;
using Telligent.Evolution.Extensibility.Version1;

namespace MetaContent
{
    public class MetaContentPlugin : IPlugin
    {
        public string Name => "MetaContent Plugin";

        public string Description => "Adds support to for generic metadata to be added to IContent Entities.";

        public void Initialize()
        {
            // TODO: Make in IInstallable

        }
    }
}
