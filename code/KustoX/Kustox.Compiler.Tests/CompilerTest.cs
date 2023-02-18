namespace Kustox.Compiler.Tests
{
    public class CompilerTest
    {
        [Fact]
        public void CaptureConstant()
        {
            var script = @"@control-flow{
    @capture-scalar myConstant = print 2
}";
            var controlFlow = new Compiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootGrouping.Blocks);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable);
            Assert.True(controlFlow.RootGrouping.Blocks.First().Capturable!.IsScalarCapture);
            Assert.Equal(
                "myConstant",
                controlFlow.RootGrouping.Blocks.First().Capturable!.CaptureName);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable!.Runnable.Query);
        }
    }
}