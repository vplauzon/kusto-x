using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public enum StepState
    {
        /// <summary>Hasn't started yet.</summary>
        Pending,
        /// <summary>Currently running.</summary>
        Running,
        /// <summary>Completed the run.</summary>
        Completed,
        /// <summary>Paused because an error was encountered.</summary>
        Error
    }
}