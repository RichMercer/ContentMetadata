using Telligent.Evolution.Extensibility.UI.Version1;

namespace ContentMetadata.Api;

public class WidgetApi : IScriptedContentFragmentExtension
{
    public string Name => "ContentMetadata Widget API";

    public string Description => "Allows access to IContent MetaData from widgets.";

    public object Extension => new ContentMetadataApi();

    public string ExtensionName => "metadata_v1_content";

    public void Initialize()
    {
    }
}