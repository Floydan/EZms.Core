using EZms.Core.AzureBlobFileProvider;
using EZms.Core.Loaders;
using EZms.Core.Repositories;
using EZms.Core.Routing;
using EZms.Core.Services;
using Rentals.Infrastructure.Routing;

namespace EZms.Core.Middleware.Models
{
    public class EZmsConfiguration
    {
        public string ConnectionString { get; set; }
        public IRouteDataCache RouteDataCache { get; set; }
        public IContentVersionRepository ContentVersionRepository { get; set; }
        public IContentRepository ContentRepository { get; set; }
        public INavigationService NavigationService { get; set; }
        public ICachedRouteDataProvider CachedRouteDataProvider { get; set; }
        public IContentLoader ContentLoader { get; set; }
        public ICachedContentTypeControllerMappings CachedPageTypeControllerMappings { get; set; }

        public AzureBlobOptions AzureBlobOptions { get; set; }
    }
}
