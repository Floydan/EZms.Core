using System;
using System.Collections.Generic;
using EZms.Core.Cache;
using EZms.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;

namespace EZms.Core.Routing
{
    public class PageConstraint : IRouteConstraint
    {
        public const string CacheKey = "__CachedRoute_GetPageList";
        private readonly object _lock = new object();

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (values[routeKey] == null) return false;

            var permalink = values[routeKey].ToString().Trim().Trim('/');
            var pageList = GetPageList();

            if (pageList.TryGetValue(permalink, out var contentId))
            {
                values.Add("contentid", contentId);
                return true;
            }

            return false;
        }

        private IDictionary<string, int> GetPageList()
        {
            lock (_lock)
            {
                if (!EZmsInMemoryCache.TryGetValue(CacheKey, out IDictionary<string, int> pages))
                {
                    var dataProvider = ServiceLocator.Current.GetInstance<ICachedRouteDataProvider>();
                    // Only allow one thread to poplate the data
                    lock (_lock)
                    {
                        if (EZmsInMemoryCache.TryGetValue(CacheKey, out pages)) return pages;

                        pages = dataProvider.GetContentToIdMap();

                        EZmsInMemoryCache.Set(CacheKey, pages,
                            new MemoryCacheEntryOptions() {
                                Priority = CacheItemPriority.NeverRemove,
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(900)
                            });
                    }
                }

                return pages;
            }
        }
    }
}
