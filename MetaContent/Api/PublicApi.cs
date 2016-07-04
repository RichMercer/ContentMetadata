using System;
using System.Collections.Generic;
using MetaContent.Data;

namespace MetaContent.Api
{
    public static class PublicApi
    {
        private const string CacheKey = "MetaData-ContentId:{0}-";

        public static IList<ContentMeta> List(Guid contentId)
        {
            // TODO: Add caching
            return DataService.List(contentId);
        }
        public static ContentMeta Get(Guid contentId, string key)
        {
            // TODO: Add caching
            return DataService.Get(contentId, key);
        }

        public static ContentMeta Set(Guid contentId, string key, string value)
        {
            // TODO: Invalidate cache
            // Consider invalidating List cache as well as Get.
            return DataService.Set(contentId, key, value);
        }
    }
}
