using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public enum ControlFlowState
    {
        Pending,
        Running,
        Completed,
        Paused,
        Error
    }
}