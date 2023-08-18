using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IStorageHub
    {
        IProcedureRunList ProcedureRunList { get; }
    }
}