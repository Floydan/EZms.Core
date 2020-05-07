using EZms.Core.Models;

namespace EZms.Core.Helpers
{
    public class PublishedStateAccessor : IPublishedStateAccessor
    {
        public bool IsPublished<T>(T content) where T : IContent
        {
            return content?.Published == true;
        }
    }
}
