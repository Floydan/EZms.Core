using System.Collections.Generic;
using System.Threading.Tasks;
using EZms.Core.Models;

namespace EZms.Core.Loaders
{
    public interface IContentLoader
    {
        Task<T> Get<T>(int id, int? version = null) where T : IContent;
        Task<IContent> GetContent(int id, int? version = null);
        Task<IEnumerable<T>> GetChildren<T>(int id) where T : IContent;
        Task<IEnumerable<int>> GetDescendents(int id);
        Task<IEnumerable<IContent>> GetAncestors(int id);
        bool TryGet<T>(int id, out T content, int? version = null) where T : IContent;
        Task<IEnumerable<IContent>> GetAll();
        Task<IEnumerable<T>> GetAll<T>() where T : IContent;
    }
}