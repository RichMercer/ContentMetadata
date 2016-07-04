using Telligent.Evolution.Extensibility.UI.Version1;

namespace MetaContent.Api
{
    public class WidgetApi : IScriptedContentFragmentExtension
    {
        public string Name => "MetaContent Widget API";

        public string Description => "Allows access to IContent MetaData from widgets.";

        public object Extension => new PublicApi();

        public string ExtensionName => "metadata_v1_content";

        public void Initialize()
        {
        }
    }
}
