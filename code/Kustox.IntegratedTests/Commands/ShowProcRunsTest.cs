using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests.Commands
{
    public class ShowProcRunsTest : TestBase
    {
        [Fact]
        public async Task ThreeFiles()
        {
            var script = ".show procedure runs";
            var result = await RunInPiecesAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Equal(3, result.Columns.Count());
            Assert.Equal(3, result.Data.Count());
        }
    }
}