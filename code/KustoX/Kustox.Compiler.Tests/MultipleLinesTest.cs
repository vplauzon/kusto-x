namespace Kustox.Compiler.Tests
{
    public class MultipleLinesTest
    {
        [Fact]
        public void CaptureQuery()
        {
            var script = @"@control-flow{
    @capture-scalar myConstant1 = print 2

    @capture-scalar myConstant2 = print 4
}";
            var controlFlow = new Compiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Equal(2, controlFlow.RootGrouping.Blocks.Count());

            //  1st constant
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable);
            Assert.True(controlFlow.RootGrouping.Blocks.First().Capturable!.IsScalarCapture);
            Assert.Equal(
                "myConstant1",
                controlFlow.RootGrouping.Blocks.First().Capturable!.CaptureName);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.First().Capturable!.Runnable.Query);

            //  2nd constant
            Assert.NotNull(controlFlow.RootGrouping.Blocks.Last().Capturable);
            Assert.True(controlFlow.RootGrouping.Blocks.Last().Capturable!.IsScalarCapture);
            Assert.Equal(
                "myConstant2",
                controlFlow.RootGrouping.Blocks.Last().Capturable!.CaptureName);
            Assert.NotNull(controlFlow.RootGrouping.Blocks.Last().Capturable!.Runnable.Query);
        }
    }
}