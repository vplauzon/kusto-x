using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.Workbench.Controllers.Language
{
    public class TableOutput
    {
        public TableOutput(TableResult result)
        {
            Columns = result.Columns
                .Select(c => new ColumnOutput
                {
                    Name = c.ColumnName,
                    Type = c.ColumnType.FullName
                })
                .ToImmutableList();
            Data = result.Data;
        }

        public IImmutableList<ColumnOutput>? Columns { get; set; }

        public IImmutableList<IImmutableList<object?>>? Data { get; set; }
    }
}