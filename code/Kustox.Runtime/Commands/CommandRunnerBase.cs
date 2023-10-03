using Kustox.Compiler.Commands;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.Commands
{
    internal abstract class CommandRunnerBase
    {
        protected CommandRunnerBase(ConnectionProvider connectionProvider)
        {
            ConnectionProvider = connectionProvider;
        }

        protected ConnectionProvider ConnectionProvider { get; }

        public abstract Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct);
    }
}