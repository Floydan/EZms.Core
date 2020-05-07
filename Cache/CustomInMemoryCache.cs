using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace EZms.Core.Cache
{
    public static class EZmsInMemoryCache
    {
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions { });
        private static readonly ConcurrentBag<object> CacheKeys = new ConcurrentBag<object>();
        private static readonly object Lock = new object();

        public static async Task<T> GetOrCreateAsync<T>(object key, Func<ICacheEntry, Task<T>> factory)
        {
            lock (Lock)
            {
                if (CacheKeys.FirstOrDefault(w => w == key) == null)
                    CacheKeys.Add(key);
            }

            return await Cache.GetOrCreateAsync(key, factory);
        }

        public static bool TryGetValue<T>(object key, out T value)
        {
            var result = Cache.TryGetValue(key, out value);

            lock (Lock)
            {
                if (result && CacheKeys.FirstOrDefault(w => w == key) == null)
                    CacheKeys.Add(key);
            }

            return result;
        }

        public static T Set<T>(object key, T value, MemoryCacheEntryOptions options)
        {
            lock (Lock)
            {
                if (CacheKeys.FirstOrDefault(w => w == key) == null)
                    CacheKeys.Add(key);
            }

            return Cache.Set(key, value, options);
        }

        public static void Remove(object key)
        {
            Cache.Remove(key);
        }

        public static void Clear()
        {
            lock (Lock)
            {
                foreach (var cacheKey in CacheKeys)
                {
                    Cache.Remove(cacheKey);
                }

                CacheKeys.Clear();
            }
        }
    }
}
