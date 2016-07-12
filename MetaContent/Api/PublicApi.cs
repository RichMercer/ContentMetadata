using System;
using System.Collections.Generic;
using ContentMetadata.Data;

namespace ContentMetadata.Api
{
    public class PublicApi
    {
        private const string CacheKey = "ContentMetadata-ContentId:{0}";
        private const string ItemCacheKey = "ContentMetadata-ContentId:{0}-Key:{1}";

        public static PublicApi Instance => new PublicApi();

        public IReadOnlyList<ContentMetadata> List(Guid contentId)
        {
            {
                return CacheHelper.Get(string.Format(CacheKey, contentId), () => DataService.List(contentId));
            }
        }
        public ContentMetadata Get(Guid contentId, string key)
        {
            return CacheHelper.Get(string.Format(ItemCacheKey, contentId, key), () => DataService.Get(contentId, key));
        }
        
        public void Delete(Guid contentId)
        {
            DataService.Delete(contentId);
        }

        public void Delete(Guid contentId, string key)
        {
            DataService.Delete(contentId, key);
        }

        public ContentMetadata Set(Guid contentId, Guid contentTypeId, string key, string value)
        {
            // Invalidate the cache
            CacheHelper.Remove(string.Format(CacheKey, contentId));
            CacheHelper.Remove(string.Format(ItemCacheKey, contentId, key));

            return DataService.Set(contentId, contentTypeId, key, value);

        }
    }
}
