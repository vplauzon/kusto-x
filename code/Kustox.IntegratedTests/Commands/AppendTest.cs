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
            var script = @$"{{
    .drop table AppendQuery ifexists

    .create-merge table AppendQuery(Id:string)

    .append AppendQuery <|
        datatable(Id:string) [""Alice"", ""Bob""]

    AppendQuery
}}";
            var output = await RunInPiecesAsync(script, null);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Single(output.Result.Columns);
            Assert.Equal(2, output.Result.Data.Count());
            Assert.Equal("Alice", output.Result.Data[0][0]);
            Assert.Equal("Bob", output.Result.Data[1][0]);
        }

        [Fact]
        public async Task AppendCaptureName()
        {
            var script = @$"{{
    .drop table AppendQuery ifexists

    .create-merge table AppendQuery(Id:string)

    @capture myData = datatable(Id:string) [""Alice"", ""Bob""]

    .append AppendQuery <|
        myData

    AppendQuery
}}";
            var output = await RunInPiecesAsync(script, null);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Single(output.Result.Columns);
            Assert.Equal(2, output.Result.Data.Count());
            Assert.Equal("Alice", output.Result.Data[0][0]);
            Assert.Equal("Bob", output.Result.Data[1][0]);
        }

        [Fact]
        public async Task AppendTransformedData()
        {
            var script = @$"{{
    .drop table AppendTransform ifexists

    .create-merge table AppendTransform(Id:string, IsMember:boolean)

    @capture myData = datatable(Id:string) [""Alice"", ""Bob""]

    .append AppendTransform <|
        myData
        | extend IsMember = true

    AppendTransform
}}";
            var output = await RunInPiecesAsync(script, null);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Equal(2, output.Result.Columns.Count);
            Assert.Equal(2, output.Result.Data.Count());
            Assert.Equal("Alice", output.Result.Data[0][0]);
            Assert.Equal(true, output.Result.Data[0][1]);
            Assert.Equal("Bob", output.Result.Data[1][0]);
            Assert.Equal(true, output.Result.Data[1][1]);
        }
    }
}