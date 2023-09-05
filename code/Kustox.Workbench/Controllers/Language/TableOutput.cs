using System.Collections.Immutable;

namespace Kustox.Workbench.Controllers.Language
{
    public class TableOutput
    {
        public IImmutableList<ColumnOutput>? Columns { get; set; }

        public IImmutableList<IImmutableList<object>>? Data { get; set; }
    }
}