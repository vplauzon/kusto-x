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
            var script = @"{
    @capture-scalar myConstant = print 2

    @capture-scalar myConstant2 = print dynamic({'Name':'Test', 'Quantity':45.4})

}";

            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task CaptureThenUseTable()
        {
            var script = @"{
    @capture myVersion = .show version

    @capture-scalar myConstant = myVersion | project ServiceType

}";

            await RunInPiecesAsync(script);
        }
    }
}