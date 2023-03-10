using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;

namespace Kustox.IntegratedTests
{
    public class ForEachTest : TestBase
    {
        [Fact]
        public async Task LoopRangeEmptySequence()
        {
            var script = @"@control-flow{
    @capture-scalar myRange = print range(0, 2, 1)

    @foreach(i in myRange){
    }
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance, null);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Single(result.Data);
            Assert.Single(result.Data.First());
        }

        [Fact]
        public async Task LoopRangePrintSequence()
        {
            var script = @"@control-flow{
    @capture-scalar myRange = print range(0, 2, 1)

    @foreach(i in myRange){
        print toint(i)
    }
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Equal(typeof(int), result.Columns[0].ColumnType);

            Assert.True(Enumerable.SequenceEqual(
                result.GetColumnData(0),
                //  Although we cast to int in Kusto, the JSON representation deserialize in long
                Enumerable.Range(0, 3).Select(i => (object)((long)i))));
        }
    }
}