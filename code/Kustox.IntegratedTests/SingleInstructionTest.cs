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
            var script = @".run-procedure <| {  }";
            
            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task CapturePrint()
        {
            var script = @".run-procedure <| {
    @capture-scalar myConstant = print 2
}";

            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task CaptureShowVersion()
        {
            var script = @".run-procedure <| {
    @capture myVersion = .show version
}";

            await RunInPiecesAsync(script);
        }

        [Fact]
        public async Task NoCaptureShowVersion()
        {
            var script = @".run-procedure <| {
    .show version
}";

            await RunInPiecesAsync(script);
        }
    }
}