using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
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
        private readonly BlobContainerClient _containerClient;

        #region Constructors
        public BlobStorageHub(
            Uri rootFolderUri,
            ClientSecretCredential credential)
        {
            var builder = new BlobUriBuilder(rootFolderUri);

            //  Enforce blob storage API
            builder.Host =
                builder.Host.Replace(".dfs.core.windows.net", ".blob.core.windows.net");

            var blobClient = new BlobClient(rootFolderUri, credential);

            _rootFolder = new DataLakeDirectoryClient(builder.ToUri(), credential);
            _containerClient = blobClient.GetParentBlobContainerClient();
        }
        #endregion

        IProcedureRunList IStorageHub.ProcedureRunList => new BlobProcedureList();

        IProcedureRunRegistry IStorageHub.ProcedureRunRegistry => new BlobProcedureRunRegistry(
            _rootFolder.GetSubDirectoryClient("runs"),
            _containerClient);
    }
}