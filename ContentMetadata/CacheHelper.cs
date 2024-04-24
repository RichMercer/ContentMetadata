using System;
using Telligent.Evolution.Extensibility.Caching.Version1;

namespace ContentMetadata;

public static class CacheHelper
{
    public static T Get<T>(string key, Func<T> getFunction, CacheScope scope = CacheScope.Context | CacheScope.Process)
    {
        return Get(key, getFunction, TimeSpan.FromSeconds(100), scope);
    }

    public static T Get<T>(string key, Func<T> getFunction, TimeSpan timeout, CacheScope scope = CacheScope.Context | CacheScope.Process)
    {
        return CacheService.Get(key, scope, getFunction, new GetOptions<T> { ExpiresAfter = timeout });
    }

    public static void Remove(string key, CacheScope scope = CacheScope.Context | CacheScope.Process)
    {
        CacheService.Remove(key, scope);
    }
}