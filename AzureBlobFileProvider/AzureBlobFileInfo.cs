using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage.Blob;
using Nito.AsyncEx;

namespace EZms.Core.AzureBlobFileProvider
{
    public class AzureBlobFileInfo : IFileInfo
    {
        private readonly CloudBlockBlob _blockBlob;
        private readonly string[] _sizes = { "B", "KB", "MB", "GB", "TB" };

        public AzureBlobFileInfo(IListBlobItem blob)
        {
            switch (blob)
            {
                case CloudBlobDirectory d:  
                    Exists = true;
                    IsDirectory = true;
                    Name = ((CloudBlobDirectory)blob).Prefix.TrimEnd('/');
                    PhysicalPath = d.StorageUri.PrimaryUri.ToString();
                    break;

                case CloudBlockBlob b:
                    _blockBlob = b;
                    Name = !string.IsNullOrEmpty(b.Parent.Prefix) ? b.Name.Replace(b.Parent.Prefix, "") : b.Name;
                    Exists = AsyncContext.Run(() => b.ExistsAsync());
                    if (Exists)
                    {
                        AsyncContext.Run(() => b.FetchAttributesAsync());
                        Length = b.Properties.Length;
                        PhysicalPath = b.Uri.ToString();
                        LastModified = b.Properties.LastModified ?? DateTimeOffset.MinValue;
                    }
                    else
                    {
                        Length = -1;
                        // IFileInfo.PhysicalPath docs say: Return null if the file is not directly accessible.
                        // (PhysicalPath should maybe also be null for blobs that do exist but that would be a potentially breaking change.)
                        PhysicalPath = null;
                    }
                    break;
            }
        }

        public Stream CreateReadStream()
        {
            var stream = new MemoryStream();
            AsyncContext.Run(() => _blockBlob.DownloadToStreamAsync(stream));
            stream.Position = 0;
            return stream;
        }

        public bool Exists { get; }
        public long Length { get; }
        public string PhysicalPath { get; }
        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public bool IsDirectory { get; }

        public string Size
        {
            get
            {
                var size = (decimal) Length;
                var order = 0;
                while (size >= 1024 && order < _sizes.Length - 1)
                {
                    order++;
                    size = size / 1024;
                }
 
                // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                // show a single decimal place, and no space.
                return $"{size:0.##} {_sizes[order]}";
            }
        }
    }
}
