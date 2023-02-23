using Kustox.Compiler;
using Kustox.CosmosDbState.DataObjects;
using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.CosmosDbState
{
    internal class CosmosDbControlFlowInstance : IControlFlowInstance
    {
        private readonly Container _container;
        private readonly long _jobId;

        public CosmosDbControlFlowInstance(Container container, long jobId)
        {
            _container = container;
            _jobId = jobId;
        }

        long IControlFlowInstance.JobId => _jobId;

        async Task IControlFlowInstance.CreateInstanceAsync(
            ControlFlowDeclaration declaration,
            CancellationToken ct)
        {
            var declarationData = new DeclarationData(_jobId, declaration);
            var stateData = new ControlFlowStateData(_jobId, ControlFlowState.Running);
            var batch = _container.CreateTransactionalBatch(
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId));

            batch.CreateItem(declarationData);
            batch.CreateItem(stateData);
            await batch.ExecuteAsync(ct);
        }

        Task IControlFlowInstance.DeleteAsync(CancellationToken ct)
        {
            //  See https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example
            //  For preview
            //await _container.DeleteAllItemsByPartitionKeyStreamAsync(ct);
            throw new NotImplementedException();
        }

        async Task<ControlFlowDeclaration> IControlFlowInstance.GetDeclarationAsync(
            CancellationToken ct)
        {
            var response = await _container.ReadItemAsync<DeclarationData>(
                DeclarationData.GetId(_jobId),
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
            var declaration = response.Resource.Declaration;

            if (declaration == null)
            {
                throw new InvalidDataException($"No declaration for job ID '{_jobId}'");
            }
            declaration.Validate();

            return declaration;
        }

        async Task<TimestampedData<ControlFlowState>> IControlFlowInstance.GetControlFlowStateAsync(
            CancellationToken ct)
        {
            var response = await _container.ReadItemAsync<ControlFlowStateData>(
                ControlFlowStateData.GetId(_jobId),
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
            var stateData = response.Resource;

            if (stateData == null)
            {
                throw new InvalidDataException($"No control flow state data for job ID '{_jobId}'");
            }

            var result = TimestampedData.Create(stateData.GetState(), response.Resource._ts);

            return result;
        }

        async Task IControlFlowInstance.SetControlFlowStateAsync(
            ControlFlowState state,
            CancellationToken ct)
        {
            var data = new ControlFlowStateData(_jobId, state);
            
            await _container.UpsertItemAsync(
                data,
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
        }

        async Task<IImmutableList<ControlFlowStep>> IControlFlowInstance.GetStepsAsync(
            CancellationToken ct)
        {
            await Task.CompletedTask;

            return ImmutableArray<ControlFlowStep>.Empty;
        }

        async Task IControlFlowInstance.CompleteStepAsync(
            IImmutableList<int> indexes,
            string captureName,
            bool isScalarCapture,
            DataTable captureTable,
            CancellationToken ct)
        {
            var data = new StepData(_jobId, indexes, captureName, isScalarCapture, captureTable);

            await _container.CreateItemAsync(
                data,
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
        }
    }
}