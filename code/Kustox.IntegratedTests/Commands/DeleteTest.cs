using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests.Commands
{
    public class DeleteTest : TestBase
    {
        [Fact]
        public async Task DeleteSomeData()
        {
            var script = @$"{{
    .drop table DeleteData ifexists

    .set DeleteData <|
        datatable (Colour: string, Id: long) [
            ""Blue"",long(42),
            ""Red"",long(43)
        ]

    .delete table DeleteData records <|
        DeleteData
        | where Colour==""Blue""

    DeleteData

}}";
            var output = await RunInPiecesAsync(script, null);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Equal(2, output.Result.Columns.Count);
            Assert.Single(output.Result.Data);
            Assert.Equal("Red", output.Result.Data[0][0]);
            Assert.Equal((long)43, output.Result.Data[0][1]);
        }
    }
}