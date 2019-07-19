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
            var cacheKey = $"GetMetadata-contentId:{content.ContentId}";
            return CacheHelper.Get(cacheKey, () => Apis.Get<IContentMetadataApi>().List(content.ContentId));
        }

        public static string GetMetadata(this IContent content, string key)
        {
            var cacheKey = $"GetMetadata-contentId:{content.ContentId}-key:{key}";
            return CacheHelper.Get(cacheKey, () => Apis.Get<IContentMetadataApi>().Get(content.ContentId, key).Value);
        }

        public static int GetMetadataInt(this IContent content, string key, int defaultValue)
        {
            if (!string.IsNullOrEmpty(content.GetMetadata(key)) && int.TryParse(content.GetMetadata(key), out var outValue))
                return outValue;

            return defaultValue;
        }

        public static bool GetMetadataBool(this IContent content, string key, bool defaultValue)
        {
            if (!string.IsNullOrEmpty(content.GetMetadata(key)) && bool.TryParse(content.GetMetadata(key), out var outValue))
                return outValue;

            return defaultValue;
        }

        public static void SetMetadata(this IContent content, string key, string value)
        {
            Apis.Get<IContentMetadataApi>().Set(content.ContentId, content.ContentTypeId, key, value);
        }
    }
}
