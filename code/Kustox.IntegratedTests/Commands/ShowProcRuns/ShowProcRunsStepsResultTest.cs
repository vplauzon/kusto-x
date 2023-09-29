using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsResultTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var procScript = @"{
    print 'a'

    print 'b'

    print 'c'
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps [1] result";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Single(result.Data.First());
            Assert.Equal("b", result.AlignDataWithNativeTypes().Data[0][0]);
        }

        [Fact]
        public async Task WithQuery()
        {
            var procScript = @"{
    print 1

    .show version

    print 3
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps [1] result | project ServiceType";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Single(result.Data.First());
            Assert.Equal("Engine", result.AlignDataWithNativeTypes().Data[0][0]);
        }
    }
}