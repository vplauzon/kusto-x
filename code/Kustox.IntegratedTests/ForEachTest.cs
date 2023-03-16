using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests
{
    public class ForEachTest : TestBase
    {
        [Fact]
        public async Task RangeEmptySequence()
        {
            var script = @"@run-procedure{
    @capture-scalar myRange = print range(0, 2, 1)

    @foreach(i in myRange){
    }
}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Single(result.Data);
            Assert.Single(result.Data.First());
        }

        [Fact]
        public async Task RangePrintSequence()
        {
            var script = @"@run-procedure{
    @capture-scalar myRange = print range(0, 2, 1)

    @foreach(i in myRange){
        print toint(i)
    }
}";
            var flowInstance = CreateControlFlowInstance();

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

        [Fact]
        public async Task ConcurrentRangePrintSequence()
        {
            var script = @"@run-procedure{
    @capture-scalar myRange = print range(0, 6, 1)

    @foreach(i in myRange) with(concurrency=3){
        print toint(i)
    }
}";
            //var numberOfSteps = new[] { 100, 5, 1 };
            var numberOfSteps = new[] { 5, 1 };

            foreach (var n in numberOfSteps)
            {
                var flowInstance = CreateControlFlowInstance();

                await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

                var result = await RunInPiecesAsync(flowInstance, n);

                Assert.NotNull(result);
                Assert.False(result.IsScalar);
                Assert.Single(result.Columns);
                Assert.Equal(typeof(int), result.Columns[0].ColumnType);

                var resultData = result.GetColumnData(0).Cast<long>().ToImmutableArray();

                Assert.Equal(7, resultData.Count());
                foreach (var i in Enumerable.Range(0, 7))
                {
                    Assert.True(
                        resultData.Contains(i),
                        $"Can't find number {i} when using {n} steps");
                }
            }
        }
    }
}