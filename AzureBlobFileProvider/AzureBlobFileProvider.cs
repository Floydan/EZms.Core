using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace EZms.Core.AzureBlobFileProvider
{
    public class AzureBlobFileProvider : IFileProvider
    {
        private readonly IBlobContainerFactory _blobContainerFactory;
        public string DocumentContainer { get; }

        public AzureBlobFileProvider(IBlobContainerFactory blobContainerFactory)
        {
            _blobContainerFactory = blobContainerFactory;
        }

        public AzureBlobFileProvider(AzureBlobOptions azureBlobOptions)
        {
            _blobContainerFactory = new DefaultBlobContainerFactory(azureBlobOptions);
            DocumentContainer = azureBlobOptions.DocumentContainer;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var container = _blobContainerFactory.GetContainer();
            var blob = container.GetDirectoryReference(_blobContainerFactory.TransformPath(subpath));
            return new AzureBlobDirectoryContents(blob);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var container = _blobContainerFactory.GetContainer();
            var blob = container.GetBlockBlobReference(_blobContainerFactory.TransformPath(subpath));
            return new AzureBlobFileInfo(blob);
        }

        public IChangeToken Watch(string filter) => throw new NotImplementedException();
    }
}
