using Azure.Storage.Files.DataLake;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.BlobStorageState
{
    public class BlobStorageHub : IStorageHub
    {
        private readonly DataLakeDirectoryClient _rootFolder;

        #region Constructors
        public BlobStorageHub(DataLakeDirectoryClient rootFolder)
        {
            _rootFolder = rootFolder;
        }
        #endregion

        IProcedureRunList IStorageHub.ProcedureRunList =>
            new BlobProcedureList(_rootFolder.GetSubDirectoryClient("runs"));
    }
}