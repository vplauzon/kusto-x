using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Newtonsoft.Json.Linq;
using System.Data.SqlTypes;

namespace Kustox.IntegratedTests
{
    public class CaptureTableTest : TestBase
    {
        [Fact]
        public async Task Bool()
        {
            await TestTableProjectionAsync("Bool", true);
        }

        [Fact]
        public async Task DateTime()
        {
            await TestTableProjectionAsync("Datetime", new DateTime(2023, 03, 01));
        }

        [Fact]
        public async Task Dynamic()
        {
            await TestTableProjectionAsync("tostring(Dynamic.user)", "bit");
        }

        [Fact]
        public async Task Guid()
        {
            await TestTableProjectionAsync(
                "Guid",
                new Guid("8c6a39f1-d14d-4f3b-85b7-b4be17fbd5bc"));
        }

        [Fact]
        public async Task Int()
        {
            await TestTableProjectionAsync("Int", (int)42);
        }

        [Fact]
        public async Task Long()
        {
            await TestTableProjectionAsync("Long", (long)1234567890123);
        }

        [Fact]
        public async Task Real()
        {
            await TestTableProjectionAsync("Real", (double)42.83);
        }

        [Fact]
        public async Task String()
        {
            await TestTableProjectionAsync("String", "Hello");
        }

        [Fact]
        public async Task TimeSpan()
        {
            await TestTableProjectionAsync("TimeSpan", System.TimeSpan.FromMinutes(4));
        }

        [Fact]
        public async Task Decimal()
        {
            await TestTableProjectionAsync("Decimal", (decimal)42.43);
        }

        private async Task TestTableProjectionAsync(
            string projection,
            object value,
            int? maximumNumberOfSteps = 1)
        {
            var script = @$"@control-flow{{
    @capture myTable = print Bool=true, Datetime=datetime(2023-03-01),
        Dynamic=dynamic({{ ""user"":""bit"", ""events"":[1,2,3], ""profile"": {{""memory"": 42}} }}),
        Guid=guid(8c6a39f1-d14d-4f3b-85b7-b4be17fbd5bc), Int=int(42), Long=1234567890123,
        Real=42.83, String='Hello', TimeSpan=4m, Decimal=decimal(42.43)

    @capture-scalar myConstant2 = myTable | project {projection}
}}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance, maximumNumberOfSteps);

            Assert.NotNull(result);
            Assert.Equal(value, result.Data[0][0]);
        }
    }
}