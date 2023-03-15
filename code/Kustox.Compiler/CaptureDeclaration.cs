namespace Kustox.Compiler
{
    public class CaptureDeclaration : DeclarationBase
    {
        public string CaptureName { get; set; } = string.Empty;

        public bool IsScalarCapture { get; set; } = false;

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(CaptureName))
            {
                throw new InvalidDataException("Capture has no name");
            }
        }
    }
}