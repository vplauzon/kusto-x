using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests.Commands
{
    public class ShowProcRunsTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var script = ".show procedure runs";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
        }
    }
}