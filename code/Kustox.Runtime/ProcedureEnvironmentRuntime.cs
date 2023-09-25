using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    public class ProcedureEnvironmentRuntime : IProcedureQueue
    {
        private readonly IProcedureRunStore _procedureRunStore;
        private readonly IProcedureRunStepRegistry _procedureRunRegistry;

        public ProcedureEnvironmentRuntime(
            IProcedureRunStore procedureRunStore,
            IProcedureRunStepRegistry procedureRunRegistry,
            ConnectionProvider connectionProvider)
        {
            _procedureRunStore = procedureRunStore;
            _procedureRunRegistry = procedureRunRegistry;
            RunnableRuntime = new RunnableRuntime(connectionProvider, this);
        }

        public RunnableRuntime RunnableRuntime { get; }

        public async Task StartAsync(bool doExisting, CancellationToken ct)
        {
            if (doExisting)
            {
                throw new NotSupportedException("Don't support starting existing procedures yet");
            }
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken ct)
        {
            await Task.CompletedTask;
        }

        #region IProcedureQueue methods
        async Task<IProcedureRunStepStore> IProcedureQueue.QueueProcedureAsync(
            SequenceDeclaration procedureDeclaration,
            bool doRun,
            CancellationToken ct)
        {
            var runStepStore = await PersistProcedureRunAsync(procedureDeclaration.Code, ct);

            if (doRun)
            {
                throw new NotSupportedException();
            }

            return runStepStore;
        }
        #endregion

        private async Task<IProcedureRunStepStore> PersistProcedureRunAsync(
            string script,
            CancellationToken ct)
        {
            var procedureRunStepStore = await _procedureRunRegistry.NewRunAsync(ct);
            var stepTask = procedureRunStepStore.AppendStepAsync(
                new[]
                {
                    new ProcedureRunStep(
                        script,
                        ImmutableArray<int>.Empty,
                        StepState.Completed,
                        null,
                        null,
                    DateTime.UtcNow)
                },
            ct);

            await _procedureRunStore.AppendRunAsync(
                new[]
                {
                    new ProcedureRun(
                        procedureRunStepStore.JobId,
                        ProcedureRunState.Pending,
                        DateTime.UtcNow)
                },
                ct);
            await stepTask;

            return procedureRunStepStore;
        }
    }
}