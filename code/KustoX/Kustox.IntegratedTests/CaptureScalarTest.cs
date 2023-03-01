using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Data.SqlTypes;

namespace Kustox.IntegratedTests
{
    public class CaptureScalarTest : TestBase
    {
        [Fact]
        public async Task Bool()
        {
            await TestScalarAsync("true", Convert.ToSByte(true));
        }

        [Fact]
        public async Task DateTime()
        {
            await TestScalarAsync("datetime(2023-03-01)", new DateTime(2023, 03, 01));
        }

        [Fact]
        public async Task Dynamic()
        {
            await TestScalarAsync("dynamic([1,2])", new long[] { 1, 2 });
        }

        [Fact]
        public async Task Guid()
        {
            await TestScalarAsync(
                "guid(0c6a39f1-d14d-4f3b-85b7-b4be17fbd586)",
                new Guid("guid(0c6a39f1-d14d-4f3b-85b7-b4be17fbd586)"));
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
            await TestScalarAsync("decimal(42.23)", new SqlDecimal(42.23));
        }

        private async Task TestScalarAsync(
            string kqlValue,
            object value,
            int? maximumNumberOfSteps = 1)
        {
            var script = @$"@control-flow{{
    @capture-scalar myConstant = print {kqlValue}

    @capture-scalar myConstant2 = print myConstant
}}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(controlFlow, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance, maximumNumberOfSteps);

            Assert.NotNull(result);
            Assert.Equal(value, result.Data[0][0]);
        }
    }
}