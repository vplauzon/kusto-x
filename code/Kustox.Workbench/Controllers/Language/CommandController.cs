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
        private readonly TokenCredential _credentials;

        public CommandController(
            ILogger<CommandController> logger,
            KustoxCompiler compiler,
            TokenCredential credentials)
        {
            _logger = logger;
            _compiler = compiler;
            _credentials = credentials;
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
                var runtime = GetRuntime();
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

        private RunnableRuntime GetRuntime()
        {
            var kustoCluster = Environment.GetEnvironmentVariable("kustoCluster");
            var kustoDbSandbox = Environment.GetEnvironmentVariable("kustoDb-sandbox");

            if (string.IsNullOrWhiteSpace(kustoCluster)
                || string.IsNullOrWhiteSpace(kustoDbSandbox))
            {
                throw new InvalidDataException("Kusto Cluster configuration missing");
            }
            else
            {
                var clusterUri = new Uri(kustoCluster);
                var connectionProvider = new ConnectionProvider(
                    clusterUri,
                    kustoDbSandbox,
                    _credentials);

                return new RunnableRuntime(connectionProvider);
            }
        }
    }
}