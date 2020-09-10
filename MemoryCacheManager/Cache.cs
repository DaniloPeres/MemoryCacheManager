using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryCacheManager
{
    public static class Cache
    {
        private static ConcurrentDictionary<string, ConcurrentBag<StoragedCache>> memoryCache = new ConcurrentDictionary<string, ConcurrentBag<StoragedCache>>();

        public static void AddCache(string key, object value, params object[] parameters)
        {
            AddCacheExpirationTime(key, value, default, parameters);
        }

        public static void AddCacheExpirationTime(string key, object value, TimeSpan expirationTime, params object[] parameters)
        {
            CleanExpiredCache();

            if (!memoryCache.TryGetValue(key, out var cacheItems))
            {
                cacheItems = new ConcurrentBag<StoragedCache>();
                memoryCache.TryAdd(key, cacheItems);
            }

            var cache = GetCacheByParameteresOrDefault(cacheItems, parameters);

            if (cache == null)
            {
                cache = new StoragedCache()
                {
                    Parameters = parameters,
                    Value = value,
                    ExpirationDate = GetExpirationDate(expirationTime)
                };
                cacheItems.Add(cache);
            }
            else
            {
                cache.Value = value;
                cache.ExpirationDate = GetExpirationDate(expirationTime);
            }
        }

        public static bool TryGetCache<T>(string key, out T output, params object[] parameters)
        {
            CleanExpiredCache();

            if (!memoryCache.TryGetValue(key, out var cacheItems))
            {
                cacheItems = new ConcurrentBag<StoragedCache>();
                memoryCache.TryAdd(key, cacheItems);
            }

            var cache = GetCacheByParameteresOrDefault(cacheItems, parameters);

            if (cache == null)
            {
                output = default;
                return false;
            }

            output = (T)Convert.ChangeType(cache.Value, typeof(T));
            return true;
        }

        public static T GetCacheOrRunAndCache<T>(string key, Func<T> runner, params object[] parameters)
        {
            return GetCacheOrRunAndCacheWithExpirationTime<T>(key, default, runner, parameters);
        }

        public static T GetCacheOrRunAndCacheWithExpirationTime<T>(string key, TimeSpan expirationTime, Func<T> runner, params object[] parameters)
        {
            if (TryGetCache<T>(key, out var value, parameters))
                return value;

            value = runner();

            if (!memoryCache.TryGetValue(key, out var cacheItems))
            {
                cacheItems = new ConcurrentBag<StoragedCache>();
                memoryCache.TryAdd(key, cacheItems);
            }

            cacheItems.Add(new StoragedCache()
            {
                Parameters = parameters,
                Value = value,
                ExpirationDate = GetExpirationDate(expirationTime)
            });

            return value;
        }

        public static void RemoveCache(string key, params object[] parameters)
        {
            CleanExpiredCache();
            if (memoryCache.TryGetValue(key, out var value)) {
                var cache = GetCacheByParameteresOrDefault(value, parameters);
                if (cache != null)
                {
                    memoryCache[key] = new ConcurrentBag<StoragedCache>(memoryCache[key].ToList().Where(item => item != cache));
                }
            }
        }

        public static bool ContainsCache(string key, params object[] parameters)
        {
            if (!memoryCache.TryGetValue(key, out var cacheItems))
                return false;

            var cache = GetCacheByParameteresOrDefault(cacheItems, parameters);
            return cache != null;
        }

        private static void CleanExpiredCache()
        {
            lock(memoryCache)
            {
                memoryCache.Keys.ToList().ForEach(key =>
                {
                    var items = memoryCache[key];
                    var itemsKeep = items.ToList().Where(item => !IsExpired(item));
                    if(itemsKeep.Any())
                    {
                        memoryCache[key] = new ConcurrentBag<StoragedCache>(itemsKeep);
                    } else
                    {
                        // Remove all items, remove cache
                        memoryCache.TryRemove(key, out var itemOutput);
                    }
                });
            }
        }

        private static bool IsExpired(StoragedCache cache)
        {
            return cache.ExpirationDate != default && cache.ExpirationDate < DateTime.Now;
        }

        private static DateTime GetExpirationDate(TimeSpan expirationTime)
        {
            return expirationTime == default
                ? default
                : DateTime.Now.Add(expirationTime);
        }

        private static StoragedCache GetCacheByParameteresOrDefault(ConcurrentBag<StoragedCache> cacheItems, object[] parameters)
        {
            return cacheItems.FirstOrDefault(item => {
                if (item.Parameters == null || parameters == null)
                    return item.Parameters == parameters;

                if (item.Parameters.Length != parameters.Length)
                    return false;

                for (var i = 0; i < item.Parameters.Length; i++)
                {
                    if (item.Parameters[i] == null || parameters[i] == null)
                    {
                        if (item.Parameters[i] == parameters[i])
                            return false;
                    } else
                    {
                        if (!item.Parameters[i].Equals(parameters[i]))
                            return false;
                    }
                }

                return true;
            });
        }
    }
}
