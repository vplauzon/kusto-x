using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kustox.BlobStorageState.DataObjects
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