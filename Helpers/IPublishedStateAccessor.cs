using EZms.Core.Models;

namespace EZms.Core.Helpers
{
    public interface IPublishedStateAccessor
    {
        bool IsPublished<T>(T content) where T : IContent;
    }
}
