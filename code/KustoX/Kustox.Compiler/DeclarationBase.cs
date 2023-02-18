namespace Kustox.Compiler
{
    public abstract class DeclarationBase
    {
        private readonly bool _hasCode;

        public DeclarationBase(bool hasCode = true)
        {
            _hasCode = hasCode;
        }

        public string? Code { get; set; }

        public virtual void Validate()
        {
            if (_hasCode && string.IsNullOrWhiteSpace(Code))
            {
                throw new InvalidDataException($"No '{nameof(Code)}'");
            }
        }
    }
}