using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class OneLinersTest
    {
        [Fact]
        public void Empty()
        {
            var script = @"@control-flow{  }";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Empty(controlFlow.RootSequence.Blocks);
        }

        [Fact]
        public void CaptureQuery()
        {
            var script = @"@control-flow{
    @capture-scalar myConstant = print 2
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootSequence.Blocks);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Capture);
            Assert.True(controlFlow.RootSequence.Blocks.First().Capture!.IsScalarCapture);
            Assert.Equal(
                "myConstant",
                controlFlow.RootSequence.Blocks.First().Capture!.CaptureName);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Query);
        }

        [Fact]
        public void CaptureCommand()
        {
            var script = @"@control-flow{
    @capture-scalar myVersionTable = .show version
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootSequence.Blocks);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Capture);
            Assert.True(controlFlow.RootSequence.Blocks.First().Capture!.IsScalarCapture);
            Assert.Equal(
                "myVersionTable",
                controlFlow.RootSequence.Blocks.First().Capture!.CaptureName);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Command);
        }

        [Fact]
        public void ExecCommand()
        {
            var script = @"@control-flow{
    .create table T(Id:string)
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootSequence.Blocks);
            Assert.Null(controlFlow.RootSequence.Blocks.First().Capture);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Command);
        }
    }
}