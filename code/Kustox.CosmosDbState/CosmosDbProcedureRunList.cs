using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;

namespace Kustox.CosmosDbState
{
    internal class CosmosDbProcedureRunList : IProcedureRunList
    {
        private readonly Container _container;

        public CosmosDbProcedureRunList(Container container)
        {
            _container = container;
        }

        IProcedureRun IProcedureRunList.GetInstance(long jobId)
        {
            return new CosmosDbProcedureRun(_container, jobId);
        }
    }
}