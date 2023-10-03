using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class ForEachTest
    {
        [Fact]
        public void EmptyForEach()
        {
            var script = @".run procedure <| {
    @foreach(n in names){
    }
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var blocks = statement.Command.RunProcedureCommand.RootSequence.Blocks;

            //  For each
            Assert.Null(blocks.First().Capture);
            Assert.NotNull(blocks.First().ForEach);
            Assert.Equal("n", blocks.First().ForEach!.Cursor);
            Assert.Equal("names", blocks.First().ForEach!.Enumerator);
            Assert.Empty(blocks.First().ForEach!.Sequence.Blocks);
        }

        [Fact]
        public void WithOneQueryForEach()
        {
            var script = @".run procedure <| {
    @foreach(n in names){
        print 2

    }
}";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var blocks = statement.Command.RunProcedureCommand.RootSequence.Blocks;
            
            //  For each
            Assert.Null(blocks.First().Capture);
            Assert.NotNull(blocks.First().ForEach);
            Assert.Equal("n", blocks.First().ForEach!.Cursor);
            Assert.Equal("names", blocks.First().ForEach!.Enumerator);

            var subBlocks = blocks.First().ForEach!.Sequence.Blocks;

            Assert.Single(subBlocks);
            Assert.NotNull(subBlocks.First().Query);
        }
    }
}