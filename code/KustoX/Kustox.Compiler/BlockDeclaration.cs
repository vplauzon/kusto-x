namespace Kustox.Compiler
{
    public class BlockDeclaration : DeclarationBase
    {
        public BlockDeclaration() : base(false)
        {
        }

        public CaptureDeclaration? Capturable { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Capturable == null)
            {
                throw new InvalidDataException(
                    $"At least one element of {typeof(BlockDeclaration).Name} must be present");
            }
        }
    }
}