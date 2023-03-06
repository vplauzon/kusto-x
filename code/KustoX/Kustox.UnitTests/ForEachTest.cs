using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class ForEachTest
    {
        [Fact]
        public void EmptyForEach()
        {
            var script = @"@control-flow{
    @foreach(n in names){
    }
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootSequence.Blocks);

            //  For each
            Assert.Null(controlFlow.RootSequence.Blocks.First().Capture);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().ForEach);
            Assert.Equal("n", controlFlow.RootSequence.Blocks.First().ForEach!.Cursor);
            Assert.Equal("names", controlFlow.RootSequence.Blocks.First().ForEach!.Enumerator);
            Assert.Empty(controlFlow.RootSequence.Blocks.First().ForEach!.Sequence.Blocks);
        }

        [Fact]
        public void WithOneQueryForEach()
        {
            var script = @"@control-flow{
    @foreach(n in names){
        print 2
    }
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootSequence.Blocks);

            //  For each
            Assert.Null(controlFlow.RootSequence.Blocks.First().Capture);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().ForEach);
            Assert.Equal("n", controlFlow.RootSequence.Blocks.First().ForEach!.Cursor);
            Assert.Equal("names", controlFlow.RootSequence.Blocks.First().ForEach!.Enumerator);

            var subBlocks = controlFlow.RootSequence.Blocks.First().ForEach!.Sequence.Blocks;

            Assert.Single(subBlocks);
            Assert.NotNull(subBlocks.First().Query);
        }
    }
}