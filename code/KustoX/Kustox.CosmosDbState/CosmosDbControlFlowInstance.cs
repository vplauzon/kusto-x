using Kustox.Compiler;
using Kustox.CosmosDbState.DataObjects;
using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;

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

        async Task IControlFlowInstance.CreateInstanceAsync(
            ControlFlowDeclaration declaration,
            CancellationToken ct)
        {
            var declarationData = new DeclarationData(_jobId, declaration);
            var stateData = new ControlFlowStateData(_jobId, ControlFlowState.Pending);
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
    }
}