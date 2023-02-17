using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Compiler
{
    public class ControlFlowPlan : PlanBase
    {
        public GroupingPlan? RootGrouping { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (RootGrouping == null)
            {
                throw new InvalidDataException($"No '{nameof(RootGrouping)}'");
            }
        }
    }
}