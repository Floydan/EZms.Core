using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core.Cache;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Loaders;
using EZms.Core.Models;
using EZms.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EZms.Core.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IContentRepository _contentRepository;
        private readonly IContentLoader _contentLoader;

        public NavigationService(IContentRepository contentRepository, IContentLoader contentLoader)
        {
            _contentRepository = contentRepository;
            _contentLoader = contentLoader;
        }

        public async Task<ICollection<NavigationNode>> CreateContentNavigation(int? rootId = null, bool includeCurrentPage = false, Type ofType = null)
        {
            var httpContext = ServiceLocator.Current.GetInstance<IHttpContextAccessor>().HttpContext;
            var includeUnPublished = httpContext.Items["Area"] as string == "EZMS";
            var currentContentId = httpContext.Items["contentid"] as int?;

            var allContents = (await _contentLoader.GetAll()).FilterForVisitor<IContent>(includeUnPublished: includeUnPublished);

            if (!includeUnPublished)
                allContents = allContents.Where(w => w.Published);

            if (ofType != null)
                allContents = allContents.Where(w => w.ModelType.IsSubclassOfGeneric(ofType));

            var contentList = allContents.Cast<Content>().ToList().AsReadOnly();

            var nav = new List<NavigationNode>();
            if (!rootId.HasValue)
            {
                foreach (var page in contentList.Where(w => !w.ParentId.HasValue).OrderBy(w => w.Order))
                {
                    var node = new NavigationNode {
                        Id = page.Id,
                        Name = page.Name,
                        Url = page.GetContentFullUrlSlug(),
                        IsStartPage = !page.ParentId.HasValue,
                        IsProduct = page.ModelType.IsSubclassOfGeneric(typeof(ProductContent<>)),
                        Type = page.ModelType,
                        IsPublished = page.Published,
                        Order = page.Order,
                        Children = GetContentChildrenRecursive(page.Id, contentList)
                    };

                    SetIsActiveState(node, currentContentId);
                    nav.Add(node);
                }
            }
            else
            {
                if (includeCurrentPage)
                {
                    var currentPage = contentList.FirstOrDefault(w => w.Id == rootId);
                    if (currentPage == null) return nav;
                    var node = new NavigationNode {
                        Id = currentPage.Id,
                        Name = currentPage.Name,
                        Url = currentPage.GetContentFullUrlSlug(),
                        IsStartPage = !currentPage.ParentId.HasValue,
                        IsProduct = currentPage.ModelType.IsSubclassOfGeneric(typeof(ProductContent<>)),
                        Type = currentPage.ModelType,
                        IsPublished = currentPage.Published,
                        Order = currentPage.Order,
                        Children = GetContentChildrenRecursive(currentPage.Id, contentList)
                    };

                    SetIsActiveState(node, currentContentId);
                    nav.Add(node);
                }
                else
                {
                    foreach (var page in contentList.Where(w => w.ParentId == rootId).OrderBy(w => w.Order))
                    {
                        var node = new NavigationNode {
                            Id = page.Id,
                            Name = page.Name,
                            Url = page.GetContentFullUrlSlug(),
                            IsStartPage = !page.ParentId.HasValue,
                            IsProduct = page.ModelType.IsSubclassOfGeneric(typeof(ProductContent<>)),
                            Type = page.ModelType,
                            IsPublished = page.Published,
                            Order = page.Order,
                            Children = GetContentChildrenRecursive(page.Id, contentList)
                        };

                        SetIsActiveState(node, currentContentId);
                        nav.Add(node);
                    }
                }
            }

            return nav;
        }

        private static ICollection<NavigationNode> GetContentChildrenRecursive(int parentId, IReadOnlyCollection<Content> allContents)
        {
            return allContents
                .Where(w => w.ParentId == parentId)
                .OrderBy(w => w.Order)
                .Select(page => new NavigationNode {
                    Id = page.Id,
                    Name = page.Name,
                    Url = page.GetContentFullUrlSlug(),
                    IsStartPage = !page.ParentId.HasValue,
                    IsProduct = page.ModelType.IsSubclassOfGeneric(typeof(ProductContent<>)),
                    Type = page.ModelType,
                    IsPublished = page.Published,
                    Order = page.Order,
                    Children = GetContentChildrenRecursive(page.Id, allContents)
                }).ToList();
        }

        private static void SetIsActiveState(NavigationNode node, int? currentContentId)
        {
            foreach (var child in node.Children)
            {
                SetIsActiveState(child, currentContentId);
                child.IsActive = child.Id == currentContentId || child.Children.Any(w => w.IsActive);
            }

            node.IsActive = node.Id == currentContentId || node.Children.Any(w => w.IsActive);
        }

        public async Task<IEnumerable<IContent>> GetContentAncestors(int? contentId, bool includeCurrentPage = false)
        {
            var cacheResult = await EZmsInMemoryCache.GetOrCreateAsync(
                $"NavigationService:GetContentAncestors:{contentId}:{includeCurrentPage}",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(60)).SetPriority(CacheItemPriority.Normal);
                    if (!contentId.HasValue) return Enumerable.Empty<Content>();

                    var ancestors = await _contentRepository.GetAncestors(contentId.Value, includeCurrentPage);
                    return ancestors;
                });
            return cacheResult;
        }

        public async Task<IEnumerable<IContent>> GetAncestors(Content content, bool includeCurrentPage = false)
        {
            if (content == null) return Enumerable.Empty<Content>();

            return await GetContentAncestors(content.Id, includeCurrentPage);
        }

        public async Task<string> GetContentAncestorsPath(int? contentId, bool includeCurrentPage = false)
        {
            var cacheResult = await EZmsInMemoryCache.GetOrCreateAsync(
                $"NavigationService:GetContentAncestorsPath:{contentId}:{includeCurrentPage}",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(60)).SetPriority(CacheItemPriority.Normal);
                    if (!contentId.HasValue) return string.Empty;

                    var content = await _contentRepository.GetContent(contentId);
                    if (content?.ParentId == null)
                        return content == null ? "" : content.UrlSlug;

                    var ancestors = await GetContentAncestors(contentId.Value, includeCurrentPage);

                    var ancestorPath = string.Join("/",
                            ancestors.Where(w => !string.IsNullOrEmpty(w.UrlSlug)).Select(w => w.UrlSlug))
                        .Trim()
                        .Trim('/')
                        .ToLower();

                    return ancestorPath;
                });
            return cacheResult;
        }

        private async Task<IOrderedEnumerable<NavigationNode>> GetContentChildrenRecursive(int parentId, bool includeUnPublished, Type ofType = null)
        {
            var childNodes = (await _contentRepository.GetChildren(parentId));

            var navigation = new List<NavigationNode>();
            foreach (var child in childNodes.OrderBy(w => w.Order))
            {
                if (!includeUnPublished && !child.Published || (ofType != null && !child.ModelType.IsSubclassOfGeneric(ofType)))
                    continue;

                var children = await GetContentChildrenRecursive(child.Id, includeUnPublished, ofType);

                navigation.Add(new NavigationNode {
                    Id = child.Id,
                    Name = child.Name,
                    Url = child.GetContentFullUrlSlug(),
                    IsStartPage = false,
                    IsProduct = child.ModelType.IsSubclassOfGeneric(typeof(ProductContent<>)),
                    Type = child.ModelType,
                    IsPublished = child.Published,
                    Order = child.Order,
                    Children = children.ToList()
                });
            }

            return navigation.OrderBy(w => w.Order);
        }
    }

    public interface INavigationService
    {
        Task<ICollection<NavigationNode>> CreateContentNavigation(int? rootId = null, bool includeCurrentPage = false, Type ofType = null);
        Task<IEnumerable<IContent>> GetContentAncestors(int? contentId, bool includeCurrentPage = false);
        Task<IEnumerable<IContent>> GetAncestors(Content content, bool includeCurrentPage = false);
        Task<string> GetContentAncestorsPath(int? contentId, bool includeCurrentPage = false);
    }
}
