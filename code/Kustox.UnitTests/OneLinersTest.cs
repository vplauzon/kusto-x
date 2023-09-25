using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class OneLinersTest
    {
        [Fact]
        public void Empty()
        {
            var script = @".run procedure <| {  }";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Empty(statement.Command.RunProcedureCommand.RootSequence.Blocks);
        }

        [Fact]
        public void CaptureQuery()
        {
            var script = @".run procedure <| {
    @capture-scalar myConstant = print 2
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var block = statement.Command.RunProcedureCommand.RootSequence.Blocks.First();

            Assert.NotNull(block.Capture);
            Assert.True(block.Capture!.IsScalarCapture);
            Assert.Equal("myConstant", block.Capture!.CaptureName);
            Assert.NotNull(block.Query);
        }

        [Fact]
        public void CaptureCommand()
        {
            var script = @".run procedure <| {
    @capture-scalar myVersionTable = .show version
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var block = statement.Command.RunProcedureCommand.RootSequence.Blocks.First();
            
            Assert.NotNull(block.Capture);
            Assert.True(block.Capture.IsScalarCapture);
            Assert.Equal("myVersionTable", block.Capture.CaptureName);
            Assert.NotNull(block.Command);
            Assert.NotNull(block.Command.GenericCommand);
        }

        [Fact]
        public void ExecCommand()
        {
            var script = @".run procedure <| {
    .create table T(Id:string)
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var block = statement.Command.RunProcedureCommand.RootSequence.Blocks.First();

            Assert.Null(block.Capture);
            Assert.NotNull(block.Command);
            Assert.NotNull(block.Command.GenericCommand);
        }
    }
}