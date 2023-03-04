namespace Kustox.Compiler
{
    public abstract class DeclarationCodeBase : DeclarationBase
    {
        public string Code { get; set; } = string.Empty;

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                throw new InvalidDataException($"No '{nameof(Code)}'");
            }
        }
    }
}