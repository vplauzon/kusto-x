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
            Id = GetId(jobId);
            JobId = jobId.ToString();
            State = state.ToString();
        }

        public static string GetId(long jobId)
        {
            return $"{jobId}.state";
        }

        public string Id { get; set; } = string.Empty;

        public string JobId { get; set; } = string.Empty;

        public string State { get; set; } = ControlFlowState.Pending.ToString();

        public long _ts { get; set; }

        public ControlFlowState GetState()
        {
            ControlFlowState strongState;

            if (!Enum.TryParse<ControlFlowState>(State, out strongState))
            {
                throw new InvalidDataException($"Invalid control state:  '{State}'");
            }
            else
            {
                return strongState;
            }
        }
    }
}