namespace ControlFlow.LanguageModel
{
    public class InstructionProperty
    {
        public static InstructionProperty[] Empty { get; } = new InstructionProperty[0];

        public string Id { get; set; } = string.Empty;

        public bool? Boolean { get; set; }

        public int? Integer { get; set; }
    }
}