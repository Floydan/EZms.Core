using Microsoft.WindowsAzure.Storage.Blob;

namespace EZms.Core.AzureBlobFileProvider
{
    public interface IBlobContainerFactory
    {
        CloudBlobContainer GetContainer();
        string TransformPath(string subpath);
    }
}
