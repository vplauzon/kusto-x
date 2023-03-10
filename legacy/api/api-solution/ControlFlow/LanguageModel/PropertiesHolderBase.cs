namespace ControlFlow.LanguageModel
{
    public abstract class PropertiesHolderBase
    {
        public InstructionProperty[] Properties { get; set; } = InstructionProperty.Empty;
    }
}