using Azure.Storage.Files.DataLake;
using Kustox.Runtime.State;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureList : IProcedureRunList
    {
        private readonly DataLakeDirectoryClient _dataLakeDirectoryClient;

        public BlobProcedureList(DataLakeDirectoryClient dataLakeDirectoryClient)
        {
            _dataLakeDirectoryClient = dataLakeDirectoryClient;
        }

        IProcedureRun IProcedureRunList.GetRun(long jobId)
        {
            return new BlobProcedureRun(jobId);
        }
    }
}