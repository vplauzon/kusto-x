using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
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
            BlockDeclaration block,
            bool isScalarCapture,
            CancellationToken ct);
    }
}