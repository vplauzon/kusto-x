using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kusto.Language;
using Kusto.Language.Syntax;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Linq;

namespace Kustox.Runtime
{
    public class RuntimeResult
    {
        public RuntimeResult(bool hasCompleteSuccessfully, TableResult? result)
        {
            HasCompleteSuccessfully = hasCompleteSuccessfully;
            Result = result;
        }

        public bool HasCompleteSuccessfully { get; }

        public TableResult? Result { get; }
    }
}