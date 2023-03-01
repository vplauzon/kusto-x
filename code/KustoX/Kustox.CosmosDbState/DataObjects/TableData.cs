using System.Collections.Immutable;
using System.Data;

namespace Kustox.CosmosDbState.DataObjects
{
    public class TableData
    {
        public TableData()
        {
        }

        public TableData(DataTable table, bool isScalar)
        {
            var columns = table.Columns
                .Cast<DataColumn>();

            IsScalar = isScalar;
            ColumnNames = columns
                .Select(c => c.ColumnName)
                .ToImmutableArray();
            ColumnTypes = columns
                .Select(c => c.DataType.FullName)
                .ToImmutableArray();
            Data = table.Rows
                .Cast<DataRow>()
                .Select(r => r.ItemArray.ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();
        }

        public bool IsScalar { get; set; }

        public IImmutableList<string>? ColumnNames { get; set; }

        public IImmutableList<string>? ColumnTypes { get; set; }

        public IImmutableList<IImmutableList<object>>? Data { get; set; }
    }
}