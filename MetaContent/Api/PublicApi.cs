using System;
using System.Collections.Generic;
using System.ComponentModel;
using MetaContent.Data;

namespace MetaContent.Api
{
    public class PublicApi
    {
        private const string CacheKey = "MetaData-ContentId:{0}-";

        public static PublicApi Instance => new PublicApi();

        public IList<ContentMeta> List(Guid contentId)
        {
            // TODO: Add caching
            return DataService.List(contentId);
        }
        public ContentMeta Get(Guid contentId, string key)
        {
            // TODO: Add caching
            return DataService.Get(contentId, key);
        }

        public void Delete(Guid contentId)
        {
            DataService.Delete(contentId);
        }

        public ContentMeta Set(Guid contentId, string key, string value)
        {
            // TODO: Invalidate cache
            // Consider invalidating List cache as well as Get.
            return DataService.Set(contentId, key, value);
        }
    }
}
