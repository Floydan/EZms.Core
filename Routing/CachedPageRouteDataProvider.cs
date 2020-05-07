using System.Collections.Generic;
using System.Linq;
using EZms.Core.Extensions;
using EZms.Core.Repositories;
using Rentals.Infrastructure.Routing;

namespace EZms.Core.Routing
{
    public class CachedPageRouteDataProvider : ICachedRouteDataProvider
    {
        private readonly IContentRepository _contentRepository;
        private readonly IRouteDataCache _routeDataCache;
        private readonly object _lock = new object();

        public CachedPageRouteDataProvider(IContentRepository contentRepository, IRouteDataCache routeDataCache)
        {
            _contentRepository = contentRepository;
            _routeDataCache = routeDataCache;

            GetPageList();
        }

        public IDictionary<string, int> GetContentToIdMap()
        {
            // Lookup the pages in DB
            var pages = _contentRepository.GetAll().GetAwaiter().GetResult();
            return pages.Select(page => new KeyValuePair<string, int>(page.GetContentFullUrlSlug(), page.Id))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        
        public IDictionary<string, int> GetPageList()
        {
            lock (_lock)
            {
                var pages = _routeDataCache.Get();
                if (pages == null || !pages.Keys.Any())
                {
                    pages = GetContentToIdMap();
                    _routeDataCache.Update(pages);
                }

                return pages;
            }
        }
    }
    public interface ICachedRouteDataProvider
    {
        IDictionary<string, int> GetContentToIdMap();
        IDictionary<string, int> GetPageList();
    }
}
