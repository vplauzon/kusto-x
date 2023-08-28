using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRunRegistry : IProcedureRunRegistry
    {
        private readonly DataLakeDirectoryClient _rootFolder;
        private readonly BlobContainerClient _containerClient;

        public BlobProcedureRunRegistry(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient)
        {
            _rootFolder = rootFolder;
            _containerClient = containerClient;
        }

        IProcedureRun IProcedureRunRegistry.GetRun(long jobId)
        {
            return new BlobProcedureRun(
                _rootFolder.GetSubDirectoryClient(jobId.ToString()),
                _containerClient,
                jobId);
        }
    }
}