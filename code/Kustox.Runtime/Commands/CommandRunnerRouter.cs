using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
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

        public CommandRunnerRouter(
            ConnectionProvider connectionProvider,
            IProcedureQueue procedureQueue)
        {
            _generic = new GenericCommandRunner(connectionProvider);
            _getBlobs = new GetBlobsCommandRunner(connectionProvider);
            _runProcedure = new RunProcedureCommandRunner(
                connectionProvider,
                procedureQueue);
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
            else
            {
                throw new NotSupportedException($"Command type");
            }
        }
    }
}