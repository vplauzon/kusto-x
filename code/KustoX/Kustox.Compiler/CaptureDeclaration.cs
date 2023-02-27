namespace Kustox.Compiler
{
    public class CaptureDeclaration : DeclarationBase
    {
        public string? CaptureName { get; set; }

        public bool? IsScalarCapture { get; set; }

        public RunnableDeclaration Runnable { get; set; } = new RunnableDeclaration();

        public override void Validate()
        {
            base.Validate();

            if (IsScalarCapture != null && string.IsNullOrEmpty(CaptureName))
            {
                throw new InvalidDataException(
                    $"Inconsistant capture in {typeof(CaptureDeclaration).Name}");
            }
            Runnable.Validate();
        }
    }
}