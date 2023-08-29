using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State.RunStep
{
    public enum StepState
    {
        /// <summary>Currently running.</summary>
        Running,
        /// <summary>Completed the run.</summary>
        Completed,
        /// <summary>Paused because an error was encountered.</summary>
        Error
    }
}