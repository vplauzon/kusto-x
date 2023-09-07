using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests
{
    public class ForEachTest : TestBase
    {
        [Fact]
        public async Task RangeEmptySequence()
        {
            var script = @"{
    @capture-scalar myRange = print range(0, 2, 1)

    @foreach(i in myRange){
    }
}";

            var result = await RunInPiecesAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Single(result.Data);
            Assert.Single(result.Data.First());
        }

        [Fact]
        public async Task RangePrintSequence()
        {
            var script = @"{
    @capture-scalar myRange = print range(0, 2, 1)

    @foreach(i in myRange){
        print toint(i)
    }
}";

            var result = await RunInPiecesAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Equal(typeof(int), result.Columns[0].ColumnType);

            //  Weirdest serialization going on:  all elements are JsonElement except
            //  the last one which is actually an integer!
            Assert.True(Enumerable.SequenceEqual(
                result
                .GetColumnData(0)
                .Select(e => e is JsonElement ? ((JsonElement)e).GetInt32() : (int)e),
                //  Although we cast to int in Kusto, the JSON representation deserialize in long
                Enumerable.Range(0, 3)));
        }

        [Fact]
        public async Task ConcurrentRangePrintSequence()
        {
            var script = @"{
    @capture-scalar myRange = print range(0, 6, 1)

    @foreach(i in myRange) with(concurrency=3){
        print toint(i)
    }
}";
            var numberOfSteps = new[] { 100, 5, 1 };

            foreach (var n in numberOfSteps)
            {
                var result = await RunInPiecesAsync(script);

                Assert.NotNull(result);
                Assert.False(result.IsScalar);
                Assert.Single(result.Columns);
                Assert.Equal(typeof(int), result.Columns[0].ColumnType);

                //  Weirdest serialization going on:  all elements are JsonElement except
                //  the last one which is actually an integer!
                var resultData = result.GetColumnData(0)
                    .Select(e => e is JsonElement ? ((JsonElement)e).GetInt32() : (int)e)
                    .ToImmutableArray();

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
