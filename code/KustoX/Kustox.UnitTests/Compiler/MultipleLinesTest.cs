using Kustox.Compiler;

namespace Kustox.UnitTests.Compiler
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
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Equal(2, controlFlow.RootSequence.Blocks.Count());

            //  1st constant
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Capturable);
            Assert.True(controlFlow.RootSequence.Blocks.First().Capturable!.IsScalarCapture);
            Assert.Equal(
                "myConstant1",
                controlFlow.RootSequence.Blocks.First().Capturable!.CaptureName);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Capturable!.Runnable.Query);

            //  2nd constant
            Assert.NotNull(controlFlow.RootSequence.Blocks.Last().Capturable);
            Assert.True(controlFlow.RootSequence.Blocks.Last().Capturable!.IsScalarCapture);
            Assert.Equal(
                "myConstant2",
                controlFlow.RootSequence.Blocks.Last().Capturable!.CaptureName);
            Assert.NotNull(controlFlow.RootSequence.Blocks.Last().Capturable!.Runnable.Query);
        }
    }
}