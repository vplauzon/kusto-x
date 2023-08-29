﻿using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRunStepRegistry : IProcedureRunStepRegistry
    {
        private readonly DataLakeDirectoryClient _rootFolder;
        private readonly BlobContainerClient _containerClient;

        public BlobProcedureRunStepRegistry(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient)
        {
            _rootFolder = rootFolder;
            _containerClient = containerClient;
        }

        async Task<IProcedureRunStepStore> IProcedureRunStepRegistry.NewRunAsync(CancellationToken ct)
        {
            var jobId = Guid.NewGuid().ToString();
            var run = new BlobProcedureRunStepStore(
                _rootFolder,
                _containerClient,
                jobId);

            await run.CreateIfNotExistsAsync(ct);

            return run;
        }

        Task<IProcedureRunStepStore> IProcedureRunStepRegistry.GetRunAsync(string jobId, CancellationToken ct)
        {
            var run = new BlobProcedureRunStepStore(
                _rootFolder,
                _containerClient,
                jobId);

            return Task.FromResult(run as IProcedureRunStepStore);
        }
    }
}