using System.Collections.Generic;
using ContentMetadata.Api;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Content.Version1;

namespace ContentMetadata.Extensions
{
    public static class MetaExtensions
    {
        public static IReadOnlyCollection<ContentMetadata> GetMetadata(this IContent content)
        {
            // TODO: Get items here and cache.
            return Apis.Get<IContentMetadataApi>().List(content.ContentId);
        }

        public static string GetMetadata(this IContent content, string key)
        {
            return Apis.Get<IContentMetadataApi>().Get(content.ContentId, key).Value;
        }

        public static int GetMetadataInt(this IContent content, string key, int defaultValue)
        {
            int outValue;
            if (!string.IsNullOrEmpty(content.GetMetadata(key)) && int.TryParse(content.GetMetadata(key), out outValue))
                return outValue;

            return defaultValue;
        }

        public static bool GetMetadataBool(this IContent content, string key, bool defaultValue)
        {
            bool outValue;
            if (!string.IsNullOrEmpty(content.GetMetadata(key)) && bool.TryParse(content.GetMetadata(key), out outValue))
                return outValue;

            return defaultValue;
        }

        public static void SetMetadata(this IContent content, string key, string value)
        {
            Apis.Get<IContentMetadataApi>().Set(content.ContentId, content.ContentTypeId, key, value);
        }
    }
}
