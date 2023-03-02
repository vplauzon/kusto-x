using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.CosmosDbState.DataObjects
{
    public class TableData
    {
        public TableData()
        {
        }

        public TableData(TableResult result)
        {
            IsScalar = result.IsScalar;
            ColumnNames = result.Columns
                .Select(c => c.ColumnName)
                .ToImmutableArray();
            ColumnTypes = result.Columns
                .Select(c => c.ColumnType.FullName)
                .ToImmutableArray();
            Data = result.Data;
        }

        public bool IsScalar { get; set; }

        public IImmutableList<string>? ColumnNames { get; set; }

        public IImmutableList<string>? ColumnTypes { get; set; }

        public IImmutableList<IImmutableList<object>>? Data { get; set; }
    }
}