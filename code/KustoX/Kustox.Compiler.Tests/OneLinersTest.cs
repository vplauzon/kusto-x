namespace Kustox.Compiler.Tests
{
    public class OneLinersTest
    {
        [Fact]
        public void Empty()
        {
            var script = @"@control-flow{  }";
            var controlFlow = new Compiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Empty(controlFlow.RootGrouping.Blocks);
        }

        [Fact]
        public void CaptureQuery()
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

        [Fact]
        public void CaptureCommand()
        {
            var script = @"@control-flow{
    @capture-scalar myVersionTable = .show version
}";
            var controlFlow = new Compiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootGrouping.Blocks);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable);
            Assert.True(controlFlow.RootGrouping.Blocks.First().Capturable!.IsScalarCapture);
            Assert.Equal(
                "myVersionTable",
                controlFlow.RootGrouping.Blocks.First().Capturable!.CaptureName);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable!.Runnable.Command);
        }

        [Fact]
        public void ExecCommand()
        {
            var script = @"@control-flow{
    .create table T(Id:string)
}";
            var controlFlow = new Compiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootGrouping.Blocks);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable);
            Assert.Null(controlFlow.RootGrouping.Blocks.First().Capturable!.CaptureName);
            Assert.Null(controlFlow.RootGrouping.Blocks.First().Capturable!.IsScalarCapture);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable!.Runnable.Command);
        }
    }
}