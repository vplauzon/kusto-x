using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    public class ProcedureEnvironmentRuntime : IProcedureQueue
    {
        #region Inner types
        private class ProcedurePackage
        {
            public ProcedurePackage(
                ProcedureRuntime runtime,
                CancellationTokenSource cancellationTokenSource,
                Task task)
            {
                Runtime = runtime;
                CancellationTokenSource = cancellationTokenSource;
                Task = task;
            }

            public ProcedureRuntime Runtime { get; }

            public CancellationTokenSource CancellationTokenSource { get; }

            public Task Task { get; }
        }
        #endregion

        private readonly ConcurrentDictionary<string, ProcedurePackage> _packageIndex =
            new ConcurrentDictionary<string, ProcedurePackage>();

        public ProcedureEnvironmentRuntime(
            KustoxCompiler compiler,
            IProcedureRunStore procedureRunStore,
            IProcedureRunStepRegistry procedureRunRegistry,
            ConnectionProvider connectionProvider)
        {
            Compiler = compiler;
            ProcedureRunStore = procedureRunStore;
            ProcedureRunRegistry = procedureRunRegistry;
            RunnableRuntime = new RunnableRuntime(connectionProvider, this);
        }

        public KustoxCompiler Compiler { get; }

        public IProcedureRunStore ProcedureRunStore { get; }

        public IProcedureRunStepRegistry ProcedureRunRegistry { get; }

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
                var runtime = new ProcedureRuntime(
                    Compiler,
                    ProcedureRunStore,
                    runStepStore,
                    RunnableRuntime);
                var cancellationTokenSource = new CancellationTokenSource();
                var compoundCt = CancellationTokenSource.CreateLinkedTokenSource(
                    ct,
                    cancellationTokenSource.Token);
                var readyToStartSource = new TaskCompletionSource();
                var task = RunProcedureAsync(runtime, readyToStartSource.Task, compoundCt);
                var package = new ProcedurePackage(runtime, cancellationTokenSource, task);

                _packageIndex.TryAdd(runStepStore.JobId, package);
                //  This avoids racing condition on the index, forbidding the proc
                //  to complete and try to remove itself from the index before it has
                //  been added
                readyToStartSource.SetResult();
            }

            return runStepStore;
        }

        private async Task RunProcedureAsync(
            ProcedureRuntime runtime,
            Task readyToStartTask,
            CancellationTokenSource compoundCt)
        {
            await readyToStartTask;
            await runtime.RunAsync(null, compoundCt.Token);
            _packageIndex.TryRemove(runtime.JobId, out var package);
        }
        #endregion

        private async Task<IProcedureRunStepStore> PersistProcedureRunAsync(
            string script,
            CancellationToken ct)
        {
            var procedureRunStepStore = await ProcedureRunRegistry.NewRunAsync(ct);
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

            await ProcedureRunStore.AppendRunAsync(
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