﻿using Kusto.Cloud.Platform.Data;
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
        private readonly IStorageHub _storageHub;

        public ShowProcedureRunsCommandRunner(
            ConnectionProvider connectionProvider,
            IStorageHub storageHub)
            : base(connectionProvider)
        {
            _storageHub = storageHub;
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
                    var stepStore = _storageHub.ProcedureRunRegistry.GetRun(runProc.JobId!);
                    var result = await stepStore.QueryRunResultAsync(
                        runProc.GetPipedQuery(),
                        ct);

                    return result;
                }
                else if (runProc.IsHistory)
                {
                    var result = await _storageHub.ProcedureRunStore.QueryRunHistoryAsync(
                        runProc.JobId!,
                        runProc.GetPipedQuery(),
                        ct);

                    return result;
                }
                else
                {
                    var result = await _storageHub.ProcedureRunStore.QueryRunsAsync(
                        runProc.JobId,
                        runProc.GetPipedQuery(),
                        ct);

                    return result;
                }
            }
            else if (runProc.Steps == null)
            {
                var stepStore = _storageHub.ProcedureRunRegistry.GetRun(runProc.JobId!);
                var result = await stepStore.QueryStepsAsync(runProc.GetPipedQuery(), ct);

                return result;
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