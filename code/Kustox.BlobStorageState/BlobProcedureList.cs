using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.Runtime.State;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureList : IProcedureRunList
    {
        private readonly DataLakeDirectoryClient _rootFolder;
        private readonly BlobContainerClient _containerClient;

        public BlobProcedureList(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient)
        {
            _rootFolder = rootFolder;
            _containerClient = containerClient;
        }

        IProcedureRun IProcedureRunList.GetRun(long jobId)
        {
            return new BlobProcedureRun(
                _rootFolder.GetSubDirectoryClient(jobId.ToString()),
                _containerClient,
                jobId);
        }
    }
}