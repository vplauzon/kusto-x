using Kustox.Compiler;

namespace Kustox.UnitTests
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
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Capture);
            Assert.True(controlFlow.RootSequence.Blocks.First().Capture!.IsScalarCapture);
            Assert.Equal(
                "myConstant1",
                controlFlow.RootSequence.Blocks.First().Capture!.CaptureName);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Query);

            //  2nd constant
            Assert.NotNull(controlFlow.RootSequence.Blocks.Last().Capture);
            Assert.True(controlFlow.RootSequence.Blocks.Last().Capture!.IsScalarCapture);
            Assert.Equal(
                "myConstant2",
                controlFlow.RootSequence.Blocks.Last().Capture!.CaptureName);
            Assert.NotNull(controlFlow.RootSequence.Blocks.Last().Query);
        }
    }
}