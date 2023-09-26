namespace Kustox.Compiler.Commands
{
    public class GetBlobDeclaration : DeclarationBase
    {
        public string RootUrl { get; set; } = string.Empty;

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(RootUrl))
            {
                throw new InvalidDataException("RootUrl unspecified");
            }
            if (!Uri.TryCreate(RootUrl, UriKind.Absolute, out _))
            {
                throw new InvalidDataException($"RootUrl is a url:  '{RootUrl}'");
            }
        }
    }
}