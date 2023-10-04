using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class IfTest
    {
        [Fact]
        public void EmptyThen()
        {
            var script = @".run proc <| {
    @if a {
    }
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var blocks = statement.Command.RunProcedureCommand.RootSequence.Blocks;

            //  If
            Assert.Null(blocks.First().Capture);
            Assert.NotNull(blocks.First().If);
            Assert.Equal("a", blocks.First().If!.Condition);
            Assert.Empty(blocks.First().If!.ThenSequence.Blocks);
            Assert.Null(blocks.First().If!.ElseSequence);
        }

        [Fact]
        public void TwoStatementsThen()
        {
            var script = @".run proc <| {
    @if a {
        .create table A(Id:string)

        .show version

    }
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var blocks = statement.Command.RunProcedureCommand.RootSequence.Blocks;

            //  If
            Assert.Null(blocks.First().Capture);
            Assert.NotNull(blocks.First().If);
            Assert.Equal("a", blocks.First().If!.Condition);
            Assert.Equal(2, blocks.First().If!.ThenSequence.Blocks.Count);
            Assert.Null(blocks.First().If!.ElseSequence);
        }
    }
}