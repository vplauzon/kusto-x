using Kusto.Data.Linq;
using Kustox.Compiler;
using Kustox.Runtime;

namespace Kustox.IntegratedTests
{
    public class SingleInstructionTest : TestBase
    {
        [Fact]
        public async Task Empty()
        {
            var script = @"{  }";
            
            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task CapturePrint()
        {
            var script = @"{
    @capture-scalar myConstant = print 2

}";

            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task CaptureShowVersion()
        {
            var script = @"{
    @capture myVersion = .show version

}";

            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task NoCaptureShowVersion()
        {
            var script = @"{
    .show version

}";

            await RunInPiecesAsync(script);
        }
    }
}