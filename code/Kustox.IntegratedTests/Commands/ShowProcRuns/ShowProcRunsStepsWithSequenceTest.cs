using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsWithSequenceTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var procScript = @"{
    print 1

    print 2

    print 3
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps [1]";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Contains("print 2", (string)result.GetColumnData("Script").First());
        }

        [Fact]
        public async Task WithQuery()
        {
            var procScript = @"{
    print 1

    print 2

    print 3
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps [1] | project Script";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Contains("print 2", (string)result.GetColumnData(0).First());
        }
    }
}