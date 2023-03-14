using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;

namespace Kustox.IntegratedTests
{
    public class SequenceTest : TestBase
    {
        [Fact]
        public async Task Capture2Prints()
        {
            var script = @"@run-procedure{
    @capture-scalar myConstant = print 2

    @capture-scalar myConstant2 = print dynamic({'Name':'Test', 'Quantity':45.4})
}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);
            await RunInPiecesAsync(flowInstance);
        }

        [Fact]
        public async Task CaptureThenUseTable()
        {
            var script = @"@run-procedure{
    @capture myVersion = .show version

    @capture-scalar myConstant = myVersion | project ServiceType
}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);
            await RunInPiecesAsync(flowInstance);
        }
    }
}