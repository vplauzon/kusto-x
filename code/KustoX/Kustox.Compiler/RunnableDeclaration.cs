namespace Kustox.Compiler
{
    public class RunnableDeclaration : DeclarationBase
    {
        public RunnableDeclaration() : base(false)
        {
        }

        public string? Query { get; set; }

        public string? Command { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Query == null && Command == null)
            {
                throw new InvalidDataException(
                    "Must have at least one runnale in"
                    + $" {typeof(RunnableDeclaration).Name}");
            }
        }
    }
}