using System;
using System.IO;
using System.Threading.Tasks;
using EZms.Core.AzureBlobFileProvider;

namespace EZms.Core.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IBlobContainerFactory _blobContainerFactory;
        private readonly IMimeMappingService _mimeMappingService;

        public ImageUploadService(IBlobContainerFactory blobContainerFactory, IMimeMappingService mimeMappingService)
        {
            _blobContainerFactory = blobContainerFactory;
            _mimeMappingService = mimeMappingService;
        }

        public async Task<string> UploadImage(string fileName, byte[] bytes)
        {
            var container = _blobContainerFactory.GetContainer();
            var mappedType = _mimeMappingService.Map(fileName);
            var fullPath = Path.Combine(mappedType.FileType, fileName).Replace(@"\", "/").Replace("//", "/");
            var blob = container.GetBlockBlobReference(_blobContainerFactory.TransformPath(fullPath));
            var exists = await blob.ExistsAsync();
            if (!exists)
            {
                await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                blob.Properties.ContentType = mappedType.ContentType;

                await blob.SetPropertiesAsync();
            }

            return fullPath;
        }

        public async Task<string> UploadImage(string fileName, Stream stream)
        {
            var container = _blobContainerFactory.GetContainer();
            var mappedType = _mimeMappingService.Map(fileName);
            var fullPath = Path.Combine(mappedType.FileType, fileName).Replace(@"\", "/").Replace("//", "/");
            var blob = container.GetBlockBlobReference(_blobContainerFactory.TransformPath(fullPath));
            var exists = await blob.ExistsAsync();
            if (!exists)
            {
                await blob.UploadFromStreamAsync(stream);
                blob.Properties.ContentType = mappedType.ContentType;

                await blob.SetPropertiesAsync();
            }
            else
            {
                fileName = DateTime.UtcNow.Ticks + Path.GetExtension(fileName);
                fullPath = Path.Combine(mappedType.FileType, fileName).Replace(@"\", "/").Replace("//", "/");
                blob = container.GetBlockBlobReference(_blobContainerFactory.TransformPath(fullPath));
                await blob.UploadFromStreamAsync(stream);
                blob.Properties.ContentType = mappedType.ContentType;

                await blob.SetPropertiesAsync();
            }

            return fullPath;
        }
    }

    public interface IImageUploadService
    {
        Task<string> UploadImage(string fileName, byte[] bytes);
        Task<string> UploadImage(string fileName, Stream stream);
    }
}
