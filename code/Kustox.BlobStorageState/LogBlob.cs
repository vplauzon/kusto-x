using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;

namespace Kustox.BlobStorageState
{
    internal class LogBlob
    {
        private DataLakeDirectoryClient rootFolder;
        private BlobContainerClient containerClient;
        private string v;

        public LogBlob(DataLakeDirectoryClient rootFolder, BlobContainerClient containerClient, string v)
        {
            this.rootFolder = rootFolder;
            this.containerClient = containerClient;
            this.v = v;
        }
    }
}