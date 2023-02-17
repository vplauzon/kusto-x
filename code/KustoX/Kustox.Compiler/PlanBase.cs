namespace Kustox.Compiler
{
    public abstract class PlanBase
    {
        public string? AssociatedCode { get; set; }

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(AssociatedCode))
            {
                throw new InvalidDataException($"No '{nameof(AssociatedCode)}'");
            }
        }
    }
}