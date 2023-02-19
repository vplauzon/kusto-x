namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly long _jobId;
        private readonly IControlFlowPersistency _persistency;

        public ControlFlowRuntime(long jobId, IControlFlowPersistency persistency)
        {
            _jobId = jobId;
            _persistency = persistency;
        }

        public async Task RunAsync(CancellationToken ct = default(CancellationToken))
        {
            var declaration = await _persistency.GetControlFlowDeclarationAsync(_jobId);

            declaration.Validate();
            foreach(var controlFlow in declaration.RootGrouping.Blocks)
            {
            }
        }
    }
}