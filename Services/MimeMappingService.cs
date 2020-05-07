using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.StaticFiles;

namespace EZms.Core.Services
{
    public class MimeMappingService : IMimeMappingService
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public MimeMappingService(FileExtensionContentTypeProvider contentTypeProvider)
        {
            _contentTypeProvider = contentTypeProvider;
        }

        public MappedType Map(string fileName)
        {
            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var extension = Path.GetExtension(fileName).ToLower();
            var mapped = new MappedType
            {
                ContentType = contentType,
                FileType = Regex.IsMatch(extension, @"\.(jpg|jpeg|gif|png|bmp)$",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    ? "images"
                    : "documents"
            };

            return mapped;
        }
    }

    public class MappedType
    {
        public string ContentType { get; set; }
        public string FileType { get; set; }
    }

    public interface IMimeMappingService
    {
        MappedType Map(string fileName);
    }

}
