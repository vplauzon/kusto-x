using Kustox.Compiler;
using Kustox.CosmosDbPersistency.DataObjects;
using Kustox.Runtime;
using Microsoft.Azure.Cosmos;

namespace Kustox.CosmosDbPersistency
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

        async Task IControlFlowInstance.SetupAsync(ControlFlowDeclaration declaration)
        {
            var data = new JobData(
                null,
                _jobId.ToString(),
                declaration);

            await _container.CreateItemAsync(data, new PartitionKey(_jobId.ToString()));
        }

        Task IControlFlowInstance.DeleteAsync()
        {
            //  See https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example
            //  For preview
            //await _container.DeleteAllItemsByPartitionKeyStreamAsync();
            throw new NotImplementedException();
        }

        Task<ControlFlowDeclaration> IControlFlowInstance.GetDeclarationAsync()
        {
            //declaration.Validate();
            throw new NotImplementedException();
        }
    }
}