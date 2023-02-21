using Kustox.Runtime.State;

namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly IControlFlowInstance _controlFlowInstance;

        public ControlFlowRuntime(IControlFlowInstance controlFlowInstance)
        {
            _controlFlowInstance = controlFlowInstance;
        }

        public async Task RunAsync(CancellationToken ct = default(CancellationToken))
        {
            var declaration = await _controlFlowInstance.GetDeclarationAsync();

            foreach(var controlFlow in declaration.RootGrouping.Blocks)
            {
            }
        }
    }
}