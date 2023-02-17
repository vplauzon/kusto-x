namespace Kustox.Compiler.Tests
{
    public class CompilerTest
    {
        [Fact]
        public void CaptureConstant()
        {
            var script = @"@control-flow{
    @capture-scalar myConstant = print 2";
            var plan = new Compiler().Compile(script);
        }
    }
}