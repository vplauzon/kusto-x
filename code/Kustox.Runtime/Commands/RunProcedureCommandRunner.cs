using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.Runtime.Commands
{
    internal class RunProcedureCommandRunner : CommandRunnerBase
    {
        private static readonly IImmutableList<ColumnSpecification> COLUMN_SPECS =
            ImmutableArray<ColumnSpecification>
            .Empty
            .Add(new ColumnSpecification("JobId", typeof(string)));

        private readonly IProcedureQueue _procedureQueue;

        public RunProcedureCommandRunner(
            ConnectionProvider connectionProvider,
            IProcedureQueue procedureQueue)
            : base(connectionProvider)
        {
            _procedureQueue = procedureQueue;
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            var procedureRunStepStore = await _procedureQueue.QueueProcedureAsync(
                command.RunProcedureCommand!.RootSequence,
                true,
                ct);
            var result = new TableResult(
                false,
                COLUMN_SPECS,
                ImmutableArray<IImmutableList<object>>
                .Empty
                .Add(ImmutableArray<object>.Empty.Add(procedureRunStepStore.JobId)));

            return result;
        }
    }
}