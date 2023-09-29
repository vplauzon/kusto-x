using Kusto.Cloud.Platform.Data;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text;

namespace Kustox.Runtime.Commands
{
    internal class ShowProcedureRunsCommandRunner : CommandRunnerBase
    {
        private readonly IProcedureRunStore _procedureRunStore;

        public ShowProcedureRunsCommandRunner(
            ConnectionProvider connectionProvider,
            IStorageHub storageHub)
            : base(connectionProvider)
        {
            _procedureRunStore = storageHub.ProcedureRunStore;
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            var runProc = command.ShowProcedureRuns!;

            if (!runProc.IsSteps)
            {
                if (runProc.IsResult)
                {
                    throw new NotImplementedException();
                }
                else if (runProc.IsHistory)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var result = await _procedureRunStore.QueryRunsAsync(
                        runProc.JobId,
                        runProc.GetPipedQuery(),
                        ct);

                    return result;
                }
            }
            else if (runProc.Steps == null)
            {
                throw new NotImplementedException();
            }
            else if (runProc.IsResult)
            {
                throw new NotImplementedException();
            }
            else if (runProc.IsHistory)
            {
                throw new NotImplementedException();
            }
            else if (runProc.IsChildren)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException("Should never reach here");
            }
        }
    }
}