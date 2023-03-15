namespace Kustox.Compiler
{
    public abstract class DeclarationBase
    {
        internal virtual void Validate()
        {
        }

        internal virtual void SubParsing(KustoxCompiler compiler)
        {
        }
    }
}