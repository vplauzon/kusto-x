using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;

namespace Kustox.BlobStorageState
{
    internal class JsonLogBlob<T>
    {
        private readonly LogBlob _logBlob;

        public JsonLogBlob(
            DataLakeDirectoryClient folder,
            BlobContainerClient containerClient,
            string blobName)
        {
            _logBlob = new LogBlob(folder, containerClient, blobName);
        }
    }
}