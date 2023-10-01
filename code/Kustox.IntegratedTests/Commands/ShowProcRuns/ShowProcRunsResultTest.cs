using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsResultTest : TestBase
    {
        [Fact]
        public async Task ShowVersion()
        {
            var procScript = @"{
    .show version
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' result";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Equal("Engine", result.GetColumnData("ServiceType").First());
        }

        [Fact]
        public async Task ShowVersionColumn()
        {
            var procScript = @"{
    .show version
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' result | project ServiceType";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Single(result.Columns);
            Assert.Equal("Engine", result.GetColumnData(0).First());
        }
    }
}