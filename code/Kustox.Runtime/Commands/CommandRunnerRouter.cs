using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.Commands
{
    internal class CommandRunnerRouter
    {
        private readonly CommandRunnerBase _generic;
        private readonly CommandRunnerBase _getBlobs;
        private readonly CommandRunnerBase _runProcedure;
        private readonly CommandRunnerBase _showProcedureRuns;
        private readonly CommandRunnerBase _showProcedureRunSteps;
        private readonly CommandRunnerBase _showProcedureRunStepsResult;

        public CommandRunnerRouter(
            ConnectionProvider connectionProvider,
            IStorageHub storageHub,
            IProcedureQueue procedureQueue)
        {
            _generic = new GenericCommandRunner(connectionProvider);
            _getBlobs = new GetBlobsCommandRunner(connectionProvider);
            _runProcedure = new RunProcedureCommandRunner(
                connectionProvider,
                procedureQueue);
            _showProcedureRuns = new ShowProcedureRunsCommandRunner(
                connectionProvider,
                storageHub);
            _showProcedureRunSteps = new ShowProcedureRunStepsCommandRunner(
                connectionProvider,
                storageHub);
            _showProcedureRunStepsResult = new ShowProcedureRunStepsResultCommandRunner(
                connectionProvider,
                storageHub);
        }

        public async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            if (command.GenericCommand != null)
            {
                return await _generic.RunCommandAsync(command, ct);
            }
            else if (command.GetBlobsCommand != null)
            {
                return await _getBlobs.RunCommandAsync(command, ct);
            }
            else if (command.RunProcedureCommand != null)
            {
                return await _runProcedure.RunCommandAsync(command, ct);
            }
            else if (command.ShowProcedureRuns != null)
            {
                return await _showProcedureRuns.RunCommandAsync(command, ct);
            }
            else if (command.ShowProcedureRunsSteps != null)
            {
                return await _showProcedureRunSteps.RunCommandAsync(command, ct);
            }
            else if (command.ShowProcedureRunsStepsResult != null)
            {
                return await _showProcedureRunStepsResult.RunCommandAsync(command, ct);
            }
            else
            {
                throw new NotSupportedException($"Command type");
            }
        }
    }
}