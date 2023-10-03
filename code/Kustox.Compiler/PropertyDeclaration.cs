using Kusto.Language;

namespace Kustox.Compiler
{
    public class PropertyDeclaration : DeclarationBase
    {
        public string Id { get; set; } = string.Empty;

        public bool? Boolean { get; set; }

        public int? Integer { get; set; }

        public string? String { get; set; }

        internal override void Validate()
        {
            base.Validate();

            var valueCount = (Boolean == null ? 0 : 1)
                + (Integer == null ? 0 : 1)
                + (String == null ? 0 : 1);

            if (Id == null)
            {
                throw new InvalidDataException($"No '{nameof(Id)}'");
            }
            if (valueCount == 0)
            {
                throw new InvalidDataException("No value set");
            }
            if (valueCount > 1)
            {
                throw new InvalidDataException("More than one value set");
            }
        }
    }
}