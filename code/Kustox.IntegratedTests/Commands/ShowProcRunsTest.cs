using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

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

        [Fact]
        public async Task SelectNonExisting()
        {
            var script = ".show procedure runs 'abc'";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task WithQuery()
        {
            var script = ".show procedure runs | project A='123'";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.NotEmpty(result.Data);
            Assert.Equal("123", result.Data[0][0]);
        }
    }
}