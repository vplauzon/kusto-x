using Kustox.Runtime;
using Kustox.Runtime.State.RunStep;

namespace Kustox.KustoState
{
    internal class KustoProcedureRunRegistry : IProcedureRunStepRegistry
    {
        private readonly ConnectionProvider _connectionProvider;

        public KustoProcedureRunRegistry(ConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        Task<IProcedureRunStepStore> IProcedureRunStepRegistry.GetRunAsync(
            string jobId,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        async Task<IProcedureRunStepStore> IProcedureRunStepRegistry.NewRunAsync(CancellationToken ct)
        {
            var jobId = Guid.NewGuid().ToString();
            var run = new KustoProcedureRunStepStore(_connectionProvider, jobId);

            await Task.CompletedTask;

            return run;
        }
    }
}