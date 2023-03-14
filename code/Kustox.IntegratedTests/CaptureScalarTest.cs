using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Newtonsoft.Json.Linq;
using System.Data.SqlTypes;

namespace Kustox.IntegratedTests
{
    public class CaptureScalarTest : TestBase
    {
        [Fact]
        public async Task Bool()
        {
            await TestScalarAsync("true", true);
        }

        [Fact]
        public async Task DateTime()
        {
            await TestScalarAsync("datetime(2023-03-01)", new DateTime(2023, 03, 01));
        }

        [Fact]
        public async Task Dynamic()
        {
            var script = @$"@run-procedure{{
    @capture-scalar myConstant = print dynamic(
        {{ ""user"":""bit"", ""events"":[1,2,3], ""profile"": {{""memory"": 42}} }})

    @capture-scalar myConstant2 = print toint(myConstant.profile.memory)
}}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance);

            Assert.NotNull(result);
            Assert.Equal(42, result.Data[0][0]);
        }

        [Fact]
        public async Task Guid()
        {
            await TestScalarAsync(
                "guid(0c6a39f1-d14d-4f3b-85b7-b4be17fbd586)",
                new Guid("0c6a39f1-d14d-4f3b-85b7-b4be17fbd586"));
        }

        [Fact]
        public async Task Int()
        {
            await TestScalarAsync("int(42)", (int)42);
        }

        [Fact]
        public async Task Long()
        {
            await TestScalarAsync("long(42)", (long)42);
        }

        [Fact]
        public async Task Real()
        {
            await TestScalarAsync("real(42.23)", (double)42.23);
        }

        [Fact]
        public async Task String()
        {
            await TestScalarAsync("'Hello World'", "Hello World");
        }

        [Fact]
        public async Task TimeSpan()
        {
            await TestScalarAsync("2s", System.TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task Decimal()
        {
            await TestScalarAsync("decimal(42.23)", (decimal)42.23);
        }

        private async Task TestScalarAsync(
            string kqlValue,
            object value,
            int? maximumNumberOfSteps = 1)
        {
            var script = @$"@run-procedure{{
    @capture-scalar myConstant = print {kqlValue}

    @capture-scalar myConstant2 = print myConstant
}}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance, maximumNumberOfSteps);

            Assert.NotNull(result);
            Assert.Equal(value, result.Data[0][0]);
        }
    }
}