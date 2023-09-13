using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal class ProcedureEnvironmentRuntime
    {
        private readonly IProcedureRunStore _procedureRunStore;
        private readonly IProcedureRunStepStore _procedureRunStepStore;
        private readonly RunnableRuntime _runnableRuntime;

        public ProcedureEnvironmentRuntime(
            IProcedureRunStore procedureRunStore,
            IProcedureRunStepStore procedureRunStepStore,
            RunnableRuntime runnableRuntime)
        {
            _procedureRunStore = procedureRunStore;
            _procedureRunStepStore = procedureRunStepStore;
            _runnableRuntime = runnableRuntime;
        }
    }
}