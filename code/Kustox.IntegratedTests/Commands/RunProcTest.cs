using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kustox.IntegratedTests.Commands
{
    public class RunProcTest : TestBase
    {
        [Fact]
        public async Task PickResult()
        {
            var ct = default(CancellationToken);
            var environmentRuntime = CreateEnvironmentRuntime();

            await environmentRuntime.StartAsync(false, ct);

            try
            {
                var script = @".run proc <| {
    @capture-scalar a = print 40

    @capture-scalar b = print 2;

    print a+b
}";
                var statement = Compiler.CompileStatement(script)!;
                var jobIdResult = await environmentRuntime.RunnableRuntime.RunStatementAsync(
                    statement,
                    ImmutableDictionary<string, TableResult?>.Empty,
                    ct);
                var jobId = (string)jobIdResult.Data[0][0];
                //  Timeout for completion:  account for running on a laptop tethering on mobile!
                var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(12));

                //  Prob until completion
                while (true)
                {
                    var run = await environmentRuntime.ProcedureRunStore.GetLatestRunAsync(
                        jobId,
                        cancelSource.Token);

                    Assert.NotNull(run);
                    if (run.State == ProcedureRunState.Completed)
                    {
                        var runStepStore = await environmentRuntime.ProcedureRunRegistry.GetRunAsync(
                            jobId,
                            ct);
                        var runResult = await runStepStore.GetRunResultAsync(ct);

                        Assert.NotNull(runResult);
                        Assert.False(runResult.IsScalar);
                        Assert.Single(runResult.Data);
                        Assert.Single(runResult.Data[0]);
                        Assert.Equal(42, ((JsonElement)runResult.Data[0][0]).GetInt64());
                        break;
                    }
                    Assert.True(run.State == ProcedureRunState.Running
                        || run.State == ProcedureRunState.Pending);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            finally
            {
                await environmentRuntime.StopAsync(ct);
            }
        }
    }
}