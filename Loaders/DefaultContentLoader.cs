using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core.Cache;
using EZms.Core.Extensions;
using EZms.Core.Models;
using EZms.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EZms.Core.Loaders
{
    public class DefaultContentLoader : ContentLoader
    {
        private readonly IContentRepository _repository;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly ILogger<DefaultContentLoader> _logger;

        public DefaultContentLoader(IContentRepository repository, IContentVersionRepository contentVersionRepository, ILogger<DefaultContentLoader> logger)
        {
            _repository = repository;
            _contentVersionRepository = contentVersionRepository;
            _logger = logger;
        }

        public override async Task<T> Get<T>(int id, int? version = null)
        {
            return await EZmsInMemoryCache.GetOrCreateAsync($"DefaultContentLoader:Get<{typeof(T).FullName}>({id}, {version})",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                    if (version.HasValue)
                    {
                        var cv = await _contentVersionRepository.Get<T>(version.Value, true);
                        if (cv != null)
                        {
                            return cv;
                        }
                    }

                    var content = await _repository.GetContent(id);
                    if (content == null || !content.Published || content.ModelType != typeof(T)) return default(T);
                    return content.ToType<T>();
                });
        }

        public override async Task<IContent> GetContent(int id, int? version = null)
        {
            return await EZmsInMemoryCache.GetOrCreateAsync($"DefaultContentLoader:GetContent({id}, {version})",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                    if (version.HasValue)
                    {
                        var cv = await _contentVersionRepository.GetContent(version.Value);
                        if (cv != null)
                        {
                            return (IContent)cv;
                        }
                    }

                    var content = await _repository.GetContent(id);
                    if (content == null || !content.Published) return null;
                    return (IContent)content;
                });
        }

        public override async Task<IEnumerable<T>> GetChildren<T>(int id)
        {
            return await EZmsInMemoryCache.GetOrCreateAsync($"DefaultContentLoader:GetChildren<IEnumerable<{typeof(T).FullName}>>({id})",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                    var childrenOfType = (await _repository.GetChildren<T>(id)).ToArray();
                    return childrenOfType.IsNullOrEmpty() ? Enumerable.Empty<T>() : childrenOfType;
                });
        }

        public override async Task<IEnumerable<int>> GetDescendents(int id)
        {
            return await EZmsInMemoryCache.GetOrCreateAsync(
                $"DefaultContentLoader:GetDescendents<IEnumerable<int>({id})",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                    var contentChildren = await _repository.GetChildren(id);

                    return contentChildren.Select(w => w.Id);
                });
        }

        public override async Task<IEnumerable<IContent>> GetAncestors(int id)
        {
            return await EZmsInMemoryCache.GetOrCreateAsync(
                $"DefaultContentLoader:GetAncestors<IEnumerable<IContent>({id})",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                    var ancestors = await _repository.GetAncestors(id);
                    return ancestors;
                });
        }

        public override bool TryGet<T>(int id, out T content, int? version = null)
        {
            try
            {
                content = Get<T>(id, version).GetAwaiter().GetResult();
                return content != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                content = default(T);
                return false;
            }
        }

        public override async Task<IEnumerable<IContent>> GetAll()
        {
            return await EZmsInMemoryCache.GetOrCreateAsync(
                $"DefaultContentLoader:GetAll<IEnumerable<IContent>",
                async entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                    var ancestors = await _repository.GetAll();
                    return ancestors;
                });
        }

        public override async Task<IEnumerable<T>> GetAll<T>()
        {
            return await EZmsInMemoryCache.GetOrCreateAsync(
                  $"DefaultContentLoader:GetAll<IEnumerable<{typeof(T).FullName}>",
                  async entry =>
                  {
                      entry.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                      var contents = await _repository.GetAll<T>();
                      return contents;
                  });
        }
    }
}
