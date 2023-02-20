namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly IControlFlowPersistency _persistency;

        public ControlFlowRuntime(IControlFlowPersistency persistency)
        {
            _persistency = persistency;
        }

        public async Task RunAsync(CancellationToken ct = default(CancellationToken))
        {
            var declaration = await _persistency.GetDeclarationAsync();

            foreach(var controlFlow in declaration.RootGrouping.Blocks)
            {
            }
        }
    }
}