using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests.Commands
{
    public class AppendTest : TestBase
    {
        [Fact]
        public async Task AppendQueryData()
        {
            var url = $"{SampleRootUrl}/3-files/";
            var script = @$"{{
    .drop table AppendQueryDataTable ifexists

    .create-merge table AppendQueryDataTable(Id:string)

    .append AppendQueryDataTable <|
        datatable(Id:string) [""Alice"", ""Bob""]

    AppendQueryDataTable
}}";
            var output = await RunInPiecesAsync(script, null);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Single(output.Result.Columns);
            Assert.Equal(2, output.Result.Data.Count());
            Assert.Equal("Alice", output.Result.Data[0][0]);
            Assert.Equal("Bob", output.Result.Data[1][0]);
        }
    }
}