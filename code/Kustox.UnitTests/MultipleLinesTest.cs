using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class MultipleLinesTest
    {
        [Fact]
        public void CaptureQuery()
        {
            var script = @".run procedure <| {
    @capture-scalar myConstant1 = print 2

    @capture-scalar myConstant2 = print 4
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Equal(2, statement.Command.RunProcedureCommand.RootSequence.Blocks.Count());

            var block1 = statement.Command.RunProcedureCommand.RootSequence.Blocks.First();
            var block2 = statement.Command.RunProcedureCommand.RootSequence.Blocks.Last();

            //  1st constant
            Assert.NotNull(block1.Capture);
            Assert.True(block1.Capture!.IsScalarCapture);
            Assert.Equal("myConstant1", block1.Capture!.CaptureName);
            Assert.NotNull(block1.Query);

            //  2nd constant
            Assert.NotNull(block2.Capture);
            Assert.True(block2.Capture!.IsScalarCapture);
            Assert.Equal("myConstant2", block2.Capture!.CaptureName);
            Assert.NotNull(block2.Query);
        }
    }
}