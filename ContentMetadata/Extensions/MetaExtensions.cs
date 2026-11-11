using System.Collections.Generic;
using ContentMetadata.Api;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Content.Version1;

namespace ContentMetadata.Extensions;

public static class MetaExtensions
{
    // IContentMetadataApi already caches per content item and clears that cache on Set and
    // Delete, so these helpers read through it rather than keeping a second cache that would
    // go stale after a change.
    public static IReadOnlyCollection<ContentMetadata> GetMetadata(this IContent content)
    {
        return Apis.Get<IContentMetadataApi>().List(content.ContentId);
    }

    public static string GetMetadata(this IContent content, string key)
    {
        return Apis.Get<IContentMetadataApi>().Get(content.ContentId, key).Value;
    }

    public static int GetMetadataInt(this IContent content, string key, int defaultValue)
    {
        return int.TryParse(content.GetMetadata(key), out var outValue) ? outValue : defaultValue;
    }

    public static bool GetMetadataBool(this IContent content, string key, bool defaultValue)
    {
        return bool.TryParse(content.GetMetadata(key), out var outValue) ? outValue : defaultValue;
    }

    public static void SetMetadata(this IContent content, string key, string value)
    {
        Apis.Get<IContentMetadataApi>().Set(content.ContentId, content.ContentTypeId, key, value);
    }
}