using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;

namespace Kustox.KustoState
{
    public class KustoStorageHub : IStorageHub
    {
        public KustoStorageHub(ConnectionProvider connectionProvider)
        {
            ProcedureRunStore = new KustoProcedureRunStore(connectionProvider);
            ProcedureRunRegistry = new KustoProcedureRunRegistry(connectionProvider);
        }

        public IProcedureRunStore ProcedureRunStore { get; }

        public IProcedureRunStepRegistry ProcedureRunRegistry { get; }
    }
}