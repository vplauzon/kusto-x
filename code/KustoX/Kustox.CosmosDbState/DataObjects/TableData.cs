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

        public DataTable? ToDataTable()
        {
            if (ColumnNames != null && ColumnTypes != null && Data != null)
            {
                var table = new DataTable();

                foreach (var column in ColumnNames.Zip(ColumnTypes))
                {
                    var name = column.First;
                    var typeName = column.Second;
                    var type = Type.GetType(typeName);

                    if (type == null)
                    {
                        throw new NotSupportedException($"Unsupported column type:  '{typeName}'");
                    }
                    table.Columns.Add(new DataColumn(name, type));
                }
                foreach (var dataRow in Data)
                {
                    table.Rows.Add(dataRow.ToArray());
                }

                return table;
            }
            else
            {
                return null;
            }
        }
    }
}