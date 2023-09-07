using System.Collections.Immutable;

namespace Kustox.Compiler
{
    public class ForEachDeclaration : DeclarationCodeBase
    {
        private const string CONCURRENCY_PROPERTY = "concurrency";

        public IImmutableList<PropertyDeclaration> Properties { get; set; } =
            ImmutableArray<PropertyDeclaration>.Empty;

        public string Cursor { get; set; } = string.Empty;

        public string Enumerator { get; set; } = string.Empty;

        public SequenceDeclaration Sequence { get; set; } = new SequenceDeclaration();

        public int Concurrency
        {
            get
            {
                var concurrency = Properties
                    .Where(p => p.Id == CONCURRENCY_PROPERTY)
                    .Select(p => p.Integer)
                    .FirstOrDefault();

                return concurrency ?? 1;
            }
        }

        internal override void Validate()
        {
            base.Validate();

            foreach (var property in Properties)
            {
                if (property.Id != CONCURRENCY_PROPERTY)
                {
                    throw new InvalidOperationException(
                        $"Unsupported property in for-each:  '{property.Id}'");
                }
                if (property.Integer == null)
                {
                    throw new InvalidOperationException(
                        $"Property '{CONCURRENCY_PROPERTY}' isn't of type integer in for-each");
                }
                if (property.Integer <= 0)
                {
                    throw new InvalidOperationException(
                        $"Property '{CONCURRENCY_PROPERTY}' in for-each must be positive but "
                        + $"is '{property.Integer}'");
                }
            }
        }
    }
}