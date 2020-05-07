using System.Collections.Generic;
using System.Threading.Tasks;
using EZms.Core.Models;

namespace EZms.Core.Loaders
{
    public abstract class ContentLoader : IContentLoader
    {
        public abstract Task<T> Get<T>(int id, int? version = null) where T : IContent;
        public abstract Task<IContent> GetContent(int id, int? version = null);

        public abstract Task<IEnumerable<T>> GetChildren<T>(int id) where T : IContent;

        public abstract Task<IEnumerable<int>> GetDescendents(int id);

        public abstract Task<IEnumerable<IContent>> GetAncestors(int id);

        public abstract bool TryGet<T>(int id, out T content, int? version = null) where T : IContent;
        public abstract Task<IEnumerable<IContent>> GetAll();

        public abstract Task<IEnumerable<T>> GetAll<T>() where T : IContent;
    }
}
