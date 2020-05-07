using System;
using System.Collections.Generic;
using EZms.Core.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace EZms.Core.Routing
{
    public class RouteDataCache : IRouteDataCache
    {
        private readonly string _cacheKey;
        private readonly object _lock = new object();

        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(15);

        public RouteDataCache()
        {
            _cacheKey = "__CachedRoute_GetPageList";
        }

        public IDictionary<string, int> Get()
        {
            if (!EZmsInMemoryCache.TryGetValue(_cacheKey, out IDictionary<string, int> pages))
            {
                // Only allow one thread to populate the data
                lock (_lock)
                {
                    if (EZmsInMemoryCache.TryGetValue(_cacheKey, out pages)) return pages;
                }
            }

            return pages;
        }

        public void Update(IDictionary<string, int> pages)
        {
            EZmsInMemoryCache.Set(_cacheKey, pages,
                new MemoryCacheEntryOptions {
                    Priority = CacheItemPriority.NeverRemove,
                    AbsoluteExpirationRelativeToNow = _cacheTimeout
                });
        }

        public void Clear()
        {
            EZmsInMemoryCache.Remove(_cacheKey);
            EZmsInMemoryCache.Remove(PageConstraint.CacheKey);
            EZmsInMemoryCache.Clear();
        }
    }

    public interface IRouteDataCache
    {
        IDictionary<string, int> Get();
        void Update(IDictionary<string, int> pages);
        void Clear();
    }
}
