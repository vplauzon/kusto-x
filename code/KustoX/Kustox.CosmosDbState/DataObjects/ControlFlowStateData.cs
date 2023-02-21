using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.CosmosDbState.DataObjects
{
    internal class ControlFlowStateData
    {
        public ControlFlowStateData()
        {
        }

        public ControlFlowStateData(long jobId, ControlFlowState state)
        {
            Id = $"{jobId}.state";
            JobId = jobId.ToString();
            State = state.ToString();
        }

        public string Id { get; set; } = string.Empty;

        public string JobId { get; set; } = string.Empty;

        public string State { get; set; } = ControlFlowState.Pending.ToString();
    }
}