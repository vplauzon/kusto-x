using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands
{
    public class ShowProcRunsStepsResultTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var script = ".show proc runs 'abc' steps [1,2,3] result";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
        }

        [Fact]
        public async Task WithQuery()
        {
            var script = ".show procedure runs 'abc' steps [1,2,3] result | project A='123'";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Data);
            Assert.Single(result.Columns);
            Assert.Equal("A", result.Columns[0].ColumnName);
            Assert.Equal(typeof(string), result.Columns[0].ColumnType);
            Assert.Equal("123", result.Data[0][0]);
        }
    }
}