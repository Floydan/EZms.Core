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
    public class ContentVersionRepository : IContentVersionRepository
    {
        private readonly EZmsContext _context;
        private readonly IMapper _mapper;
        private readonly IRouteDataCache _routeDataCache;

        public ContentVersionRepository(EZmsContext context, IMapper mapper, IRouteDataCache routeDataCache)
        {
            _context = context;
            _mapper = mapper;
            _routeDataCache = routeDataCache;
        }

        public async Task<T> Get<T>(int? id, bool useInPreview = false) where T : IContent
        {
            if (!id.HasValue) return default(T);
            var content = await _context.ContentVersions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.ContentTypeGuid == typeof(T).GetPageDataValues().Guid);
            if (content == null) return default(T);

            var typedContent = _mapper.Map<Content>(content).ToType<T>();
            if(useInPreview)
                typedContent.Id = content.ContentId;

            return typedContent;
        }

        public async Task<ContentVersion> GetContent(int? id)
        {
            if (!id.HasValue) return null;
            var content = await _context.ContentVersions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            return content;
        }

        public async Task<IEnumerable<ContentVersion>> GetAll(int contentId)
        {
            var versions = await _context.ContentVersions.AsNoTracking().Where(c => c.ContentId == contentId).OrderBy(w => w.Id).ToListAsync();
            return versions;
        }

        public async Task<T> Create<T>(T content) where T : IContent
        {
            content.Id = 0;
            
            var c = _mapper.Map<ContentVersion>(content);
            c.UpdatedAt = DateTime.UtcNow;

            _context.Add((object) c);

            await _context.SaveChangesAsync();

            content.Id = c.Id;

            var olderVersions = await _context.ContentVersions.Where(w => w.ContentId == c.ContentId).ToListAsync();
            if (olderVersions.Count > 10)
            {
                var sorted = olderVersions.OrderByDescending(w => w.Id);
                var tooOld = sorted.Skip(10).ToList();
                if (tooOld.Any())
                {
                    foreach (var contentVersion in tooOld)
                    {
                        _context.ContentVersions.Remove(contentVersion);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return content;
        }

        public async Task<T> Update<T>(T content) where T : IContent
        {
            var c = _mapper.Map<ContentVersion>(content);
            c.UpdatedAt = DateTime.UtcNow;

            var currentVersion = _context.ContentVersions.AsNoTracking().First(w => w.Id == c.Id);

            currentVersion = _mapper.Map(c, currentVersion);    

            if (_context.Entry(currentVersion).State == EntityState.Detached)
                _context.Attach(currentVersion);

            _context.Update(currentVersion);

            await _context.SaveChangesAsync();

            return content;
        }

        public async Task Delete(int id)
        {
            var content = await _context.ContentVersions.FindAsync(id);
            _context.ContentVersions.Remove(content);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAll(int contentId)
        {
            var contents = await _context.ContentVersions.Where(w => w.ContentId == contentId).ToListAsync();
            _context.Remove(contents);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int? id)
        {
            if (!id.HasValue) return false;
            return await _context.Content.AnyAsync(w => w.Id == id);
        }
    }

    public interface IContentVersionRepository
    {
        Task<T> Get<T>(int? id, bool useInPreview = false) where T : IContent;
        Task<ContentVersion> GetContent(int? id);
        Task<IEnumerable<ContentVersion>> GetAll(int contentId);
        Task<T> Create<T>(T content) where T : IContent;
        Task<T> Update<T>(T content) where T : IContent;
        Task Delete(int id);
        Task DeleteAll(int contentId);
        Task<bool> Exists(int? id);
    }
}
