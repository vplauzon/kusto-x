using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Newtonsoft.Json.Linq;
using System.Data.SqlTypes;
using System.Text.Json;

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
            await TestScalarAsync(
                "datetime(2023-03-01)",
                new DateTime(2023, 03, 01));
        }

        [Fact]
        public async Task Dynamic()
        {
            var script = @$"{{
    @capture-scalar myConstant = print dynamic(
        {{ ""user"":""bit"", ""events"":[1,2,3], ""profile"": {{""memory"": 42}} }})

    @capture-scalar myConstant2 = print toint(myConstant.profile.memory)
}}";
            var output = await RunInPiecesAsync(script);
            var result = output.Result;

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
            await TestScalarAsync(
                "2s",
                System.TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task Decimal()
        {
            await TestScalarAsync("decimal(42.23)", (decimal)42.23);
        }

        private async Task TestScalarAsync<T>(
            string kqlValue,
            T value,
            int? maximumNumberOfSteps = 1)
        {
            var script = @$"{{
    @capture-scalar myConstant = print {kqlValue}

    @capture-scalar myConstant2 = print myConstant
}}";
            var output = await RunInPiecesAsync(script);
            var result = output.Result;

            Assert.NotNull(result);
            Assert.Equal(value, result.Data[0][0]);
        }
    }
}