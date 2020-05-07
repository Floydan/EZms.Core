using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EZms.Core.Extensions;
using EZms.Core.Models;
using EZms.Core.Routing;
using Microsoft.EntityFrameworkCore;
using Rentals.Infrastructure.Routing;

namespace EZms.Core.Repositories
{
    public class ContentRepository : IContentRepository
    {
        private readonly EZmsContext _context;
        private readonly IMapper _mapper;
        private readonly IRouteDataCache _routeDataCache;

        public ContentRepository(EZmsContext context, IMapper mapper, IRouteDataCache routeDataCache)
        {
            _context = context;
            _mapper = mapper;
            _routeDataCache = routeDataCache;
        }

        public async Task<T> Get<T>(int? id) where T : IContent
        {
            if (!id.HasValue) return default(T);

            var content = await _context.Content.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.ContentTypeGuid == typeof(T).GetPageDataValues().Guid);
            if (content == null) return default(T);

            var typedContent = _mapper.Map<T>(content);

            return typedContent;
        }

        public async Task<Content> GetContent(int? id)
        {
            if (!id.HasValue) return null;
            var content = await _context.Content.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            return content;
        }

        public async Task<IEnumerable<Content>> GetContentWithParents(int? id)
        {
            if (!id.HasValue) return null;

            var allContent = await _context.Content.AsNoTracking().ToListAsync();

            var content = allContent.SingleOrDefault(c => c.Id == id);
            if (content == null) return null;

            var hierarchy = new List<Content> { content };

            var parentId = content.ParentId;
            while (parentId != null)
            {
                var parent = allContent.SingleOrDefault(c => c.Id == parentId);
                if (parent == null) continue;

                hierarchy.Add(parent);
                parentId = parent.ParentId;
            }

            return hierarchy;
        }

        public async Task<T> Get<T>(int? parentId, string slug) where T : IContent
        {
            var content = await _context.Content.AsNoTracking().FirstOrDefaultAsync(c =>
                c.ParentId == parentId &&
                c.UrlSlug == slug &&
                c.ContentTypeGuid == typeof(T).GetPageDataValues().Guid);
            var typedContent = _mapper.Map<T>(content);
            return typedContent;
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : IContent
        {
            var contents = (await _context.Content.AsNoTracking().Where(c => c.ContentTypeGuid == typeof(T).GetPageDataValues().Guid).ToListAsync()).Select(w => w.ToType<T>());
            return contents;
        }

        public async Task<IEnumerable<Content>> GetAll()
        {
            var contents = await _context.Content.AsNoTracking().ToListAsync();
            return contents;
        }

        public async Task<IEnumerable<IContent>> GetChildren(int? parentId)
        {
            var contents = await _context.Content.AsNoTracking().Where(c => c.ParentId == parentId).ToListAsync();
            return contents;
        }

        public async Task<IEnumerable<T>> GetChildren<T>(int? parentId) where T : IContent
        {
            var contents = await _context.Content.AsNoTracking().Where(c => c.ParentId == parentId && c.ContentTypeGuid == typeof(T).GetPageDataValues().Guid).ToListAsync();

            return contents.Select(w => w.ToType<T>());
        }

        public async Task<T> Create<T>(T content) where T : IContent
        {
            content.CreatedAt = DateTime.UtcNow;

            var c = _mapper.Map<Content>(content);

            if (c.ParentId.HasValue)
            {
                var maxOrder = _context.Content.Max(w => w.Order);
                c.Order = maxOrder + 1;
            }

            _context.Add(c);
            await _context.SaveChangesAsync();

            content.Id = c.Id;

            _routeDataCache.Clear();

            return content;
        }

        public async Task<T> Update<T>(T content) where T : IContent
        {
            content.UpdatedAt = DateTime.UtcNow;

            var dbContent = await GetContent(content.Id);
            var c = _mapper.Map<Content>(content);

            if (!_context.Exists(c))
                _context.Attach(c);

            _context.Update(c);

            await _context.SaveChangesAsync();

            if (dbContent.ParentId != content.ParentId || dbContent.UrlSlug != content.UrlSlug)
                _routeDataCache.Clear();

            return content;
        }

        public async Task UpdateParentId(int id, int parentId, bool clearRouteCache)
        {
            var dbContent = await _context.Content.FirstOrDefaultAsync(c => c.Id == id);
            if (dbContent.ParentId == parentId) return;

            dbContent.ParentId = parentId;

            _context.Update(dbContent);
            await _context.SaveChangesAsync();

            if (clearRouteCache)
                _routeDataCache.Clear();
        }

        public async Task UpdateSortOrder(int parentId, IEnumerable<int> childIds, bool clearRouteCache)
        {
            var children = await _context.Content.Where(w => w.ParentId == parentId).ToListAsync();

            var order = 0;
            foreach (var childId in childIds)
            {
                var child = children.FirstOrDefault(w => w.Id == childId);
                if (child == null) continue;

                child.Order = order;

                _context.Update(child);
                order++;
            }

            await _context.SaveChangesAsync();

            if (clearRouteCache)
                _routeDataCache.Clear();
        }

        public async Task<T> Publish<T>(T content, int? versionId = null) where T : IContent
        {
            var dbContent = await GetContent(content.Id);
            var c = _mapper.Map<Content>(content);

            if (!_context.Exists(c))
                _context.Attach(c);

            c.Published = true;

            if(versionId.HasValue)
                c.PublishedVersion = versionId.Value;

            c.PublishedAt = DateTime.UtcNow;

            _context.Update(c);

            await _context.SaveChangesAsync();

            if (dbContent.ParentId != content.ParentId || dbContent.UrlSlug != content.UrlSlug)
                _routeDataCache.Clear();

            content.Published = true;

            return content;
        }

        public async Task Publish(int id, int? versionId = null)
        {
            var dbContent = await _context.Content.FirstOrDefaultAsync(c => c.Id == id);

            dbContent.Published = true;

            if(versionId.HasValue)
                dbContent.PublishedVersion = versionId.Value;

            dbContent.PublishedAt = DateTime.UtcNow;

            _context.Update(dbContent);

            await _context.SaveChangesAsync();

            _routeDataCache.Clear();
        }

        public async Task<T> UnPublish<T>(T content) where T : IContent
        {
            var dbContent = await GetContent(content.Id);
            var c = _mapper.Map<Content>(content);

            if (!_context.Exists(c))
                _context.Attach(c);

            c.Published = false;
            c.PublishedAt = DateTime.MaxValue;

            _context.Update(c);

            await _context.SaveChangesAsync();

            if (dbContent.ParentId != content.ParentId || dbContent.UrlSlug != content.UrlSlug)
                _routeDataCache.Clear();

            content.Published = false;

            return content;
        }

        public async Task UnPublish(int id)
        {
            var dbContent = await _context.Content.FirstOrDefaultAsync(c => c.Id == id);

            if (!_context.Exists(dbContent))
                _context.Attach(dbContent);

            dbContent.Published = false;
            dbContent.PublishedAt = DateTime.MaxValue;

            _context.Update(dbContent);

            await _context.SaveChangesAsync();

            _routeDataCache.Clear();
        }

        public async Task<IEnumerable<IContent>> GetAncestors(int? id, bool includeCurrentContent = false)
        {
            if (!id.HasValue) return Enumerable.Empty<Content>();

            var ancestors = new List<Content>();

            var content = await GetContent(id);

            if (includeCurrentContent)
                ancestors.Add(content);

            if (content?.ParentId == null)
            {
                return ancestors;
            }

            var parent = await GetContent(content.ParentId);
            while (parent != null)
            {
                ancestors.Add(parent);
                parent = await GetContent(parent.ParentId);
            }

            ancestors.Reverse();

            return ancestors;
        }

        public async Task<IEnumerable<IContent>> GetAncestors(Content content)
        {
            if (content?.ParentId == null) return Enumerable.Empty<Content>();

            return await GetAncestors(content.ParentId.Value);
        }

        public async Task<IEnumerable<IContent>> GetItems(IEnumerable<int> ids)
        {
            var contents = await _context.Content.AsNoTracking().Where(c => ids.Contains(c.Id)).ToListAsync();
            return contents;
        }

        public async Task Delete(int id)
        {
            var content = await _context.Content.FindAsync(id);
            _context.Content.Remove(content);
            await _context.SaveChangesAsync();

            _routeDataCache.Clear();
        }

        public async Task<bool> Exists(int? id)
        {
            if (!id.HasValue) return false;
            return await _context.Content.AnyAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<IContent>> FindStringInModel(string value)
        {
            return await _context.Content.AsNoTracking().Where(c => c.ModelAsJson.IndexOf(value, StringComparison.Ordinal) != -1).ToListAsync();
        }
    }

    public interface IContentRepository
    {
        Task<T> Get<T>(int? id) where T : IContent;
        Task<T> Get<T>(int? parentId, string slug) where T : IContent;
        Task<Content> GetContent(int? id);
        Task<IEnumerable<T>> GetAll<T>() where T : IContent;
        Task<IEnumerable<Content>> GetAll();
        Task<IEnumerable<IContent>> GetChildren(int? parentId);
        Task<IEnumerable<T>> GetChildren<T>(int? parentId) where T : IContent;
        Task<T> Create<T>(T content) where T : IContent;
        Task<T> Update<T>(T content) where T : IContent;
        Task UpdateParentId(int id, int parentId, bool clearRouteCache);
        Task UpdateSortOrder(int parentId, IEnumerable<int> childIds, bool clearRouteCache);
        Task<T> Publish<T>(T content, int? versionId = null) where T : IContent;
        Task Publish(int id, int? versionId = null);
        Task<T> UnPublish<T>(T content) where T : IContent;
        Task UnPublish(int id);
        Task<IEnumerable<IContent>> GetAncestors(int? id, bool includeCurrentContent = false);
        Task<IEnumerable<IContent>> GetAncestors(Content content);
        Task<IEnumerable<IContent>> GetItems(IEnumerable<int> ids);
        Task Delete(int id);
        Task<bool> Exists(int? id);
        Task<IEnumerable<IContent>> FindStringInModel(string value);
    }
}
