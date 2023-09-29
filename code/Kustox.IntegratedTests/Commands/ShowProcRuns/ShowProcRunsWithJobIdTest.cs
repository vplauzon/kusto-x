using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
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
   }
}