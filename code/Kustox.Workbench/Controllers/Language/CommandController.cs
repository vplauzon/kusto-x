using Azure.Core;
using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State.RunStep;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace Kustox.Workbench.Controllers.Language
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly ILogger<CommandController> _logger;
        private readonly KustoxCompiler _compiler;
        private readonly ProcedureEnvironmentRuntime _procedureEnvironmentRuntime;

        public CommandController(
            ILogger<CommandController> logger,
            KustoxCompiler compiler,
            ProcedureEnvironmentRuntime procedureEnvironmentRuntime)
        {
            _logger = logger;
            _compiler = compiler;
            _procedureEnvironmentRuntime = procedureEnvironmentRuntime;
        }

        [HttpPost]
        public async Task<CommandOutput> PostAsync(CommandInput input, CancellationToken ct)
        {
            var statement = _compiler.CompileStatement(input.Csl);

            if (statement == null)
            {
                throw new InvalidOperationException($"Can't compile '{input.Csl}'");
            }
            else
            {
                var runtime = _procedureEnvironmentRuntime.RunnableRuntime;
                var result = await runtime.RunStatementAsync(
                    statement,
                    ImmutableDictionary<string, TableResult?>.Empty,
                    ct);

                return new CommandOutput
                {
                    Table = new TableOutput(result)
                };
            }
        }
    }
}