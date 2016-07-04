using System;
using Telligent.Evolution.Extensibility.Caching.Version1;

namespace ContentMetadata
{
    public static class CacheHelper
    {
        public static T Get<T>(string key, Func<T> getFunction, CacheScope scope = CacheScope.Context | CacheScope.Process)
        {
            return Get(key, getFunction, TimeSpan.FromSeconds(100), scope);
        }

        public static T Get<T>(string key, Func<T> getFunction, TimeSpan timeout, CacheScope scope = CacheScope.Context | CacheScope.Process)
        {
            var cachedObj = CacheService.Get(key, scope);
            if (cachedObj != null)
                return (T)cachedObj;

            var obj = getFunction();
            CacheService.Put(key, obj, scope);
            return obj;
        }

        public static void Remove(string key, CacheScope scope = CacheScope.Context | CacheScope.Process)
        {
            CacheService.Remove(key, scope);
        }
    }
}
