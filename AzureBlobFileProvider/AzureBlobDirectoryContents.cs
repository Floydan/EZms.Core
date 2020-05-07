using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage.Blob;
using Nito.AsyncEx;

namespace EZms.Core.AzureBlobFileProvider
{
    public class AzureBlobDirectoryContents : IDirectoryContents
    {
        private readonly List<IListBlobItem> _blobs = new List<IListBlobItem>();
        public bool Exists { get; set; }

        public AzureBlobDirectoryContents(CloudBlobDirectory blob)
        {
            BlobContinuationToken continuationToken = null;

            do
            {
                var token = continuationToken;
                var response = AsyncContext.Run(() => blob.ListBlobsSegmentedAsync(token));
                continuationToken = response.ContinuationToken;
                _blobs.AddRange(response.Results);
            }
            while (continuationToken != null);
            Exists = _blobs.Count > 0;
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _blobs.Select(blob => new AzureBlobFileInfo(blob)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
