using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ContentMetadata.Data;

namespace ContentMetadata.Api
{
    public class PublicApi
    {
        private const string CacheKey = "ContentMetadata-ContentId:{0}";
        private const string ItemCacheKey = "ContentMetadata-ContentId:{0}-Key:{1}";

        public static PublicApi Instance => new PublicApi();

        public IReadOnlyCollection<ContentMetadata> List(Guid contentId)
        {
            return CacheHelper.Get(string.Format(CacheKey, contentId), () => new ReadOnlyCollection<ContentMetadata>(DataService.List(contentId)));
        }
        public ContentMetadata Get(Guid contentId, string key)
        {
            return CacheHelper.Get(string.Format(ItemCacheKey, contentId, key), () => DataService.Get(contentId, key));
        }
        
        public void Delete(Guid contentId)
        {
            DataService.Delete(contentId);
        }

        public void Set(Guid contentId, Guid contentTypeId, string key, string value)
        {
            DataService.Set(contentId, contentTypeId, key, value);

            // Invalidate the cache
            CacheHelper.Remove(string.Format(CacheKey, contentId));
            CacheHelper.Remove(string.Format(ItemCacheKey, contentId, key));
        }
    }
}
