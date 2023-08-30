using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.BlobStorageState
{
    public class BlobStorageHub : IStorageHub
    {
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
            var rootFolder = new DataLakeDirectoryClient(builder.ToUri(), credential);
            var containerClient = blobClient.GetParentBlobContainerClient();

            ProcedureRunStore = new BlobProcedureRunStore(
                rootFolder.GetSubDirectoryClient("runs"),
                containerClient);
            ProcedureRunRegistry = new BlobProcedureRunStepRegistry(
                rootFolder.GetSubDirectoryClient("runs"),
                containerClient);
        }
        #endregion

        public IProcedureRunStore ProcedureRunStore { get; }

        public IProcedureRunStepRegistry ProcedureRunRegistry { get; }

    }
}