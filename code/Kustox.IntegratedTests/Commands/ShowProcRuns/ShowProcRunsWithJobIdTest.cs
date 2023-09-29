using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsWithJobIdTest : TestBase
    {
        [Fact]
        public async Task SelectNonExisting()
        {
            var script = ".show proc runs 'abc'";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task CheckState()
        {
            var procScript = @"{
    print 42
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}'";
            var result = await RunStatementAsync(showScript);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Single(result.GetColumnData("State"));
            Assert.Equal(StepState.Completed.ToString(), result.GetColumnData("State").First());
        }

        [Fact]
        public async Task CheckStateWithQuery()
        {
            var procScript = @"{
    print 42
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' | project State";
            var result = await RunStatementAsync(showScript);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Single(result.GetColumnData(0));
            Assert.Equal(StepState.Completed.ToString(), result.GetColumnData(0).First());
        }
    }
}