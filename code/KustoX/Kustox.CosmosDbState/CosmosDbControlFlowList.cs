using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;

namespace Kustox.CosmosDbState
{
    internal class CosmosDbControlFlowList : IControlFlowList
    {
        private readonly Container _container;

        public CosmosDbControlFlowList(Container container)
        {
            _container = container;
        }

        IControlFlowInstance IControlFlowList.GetInstance(long jobId)
        {
            return new CosmosDbControlFlowInstance(_container, jobId);
        }
    }
}