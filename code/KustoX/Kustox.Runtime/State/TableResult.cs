using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public class TableResult
    {

        public TableResult(
            bool isScalar,
            IImmutableList<ColumnSpecification> columns,
            IImmutableList<IImmutableList<object>> data)
        {
            IsScalar = isScalar;
            Columns = columns;
            Data = data;
        }

        public TableResult(bool isScalar, DataTable table)
        {
            IsScalar = isScalar;
            Columns = table.Columns
                .Cast<DataColumn>()
                .Select(c => new ColumnSpecification(c.ColumnName, c.DataType.FullName!))
                .ToImmutableArray();
            Data = table.Rows
                .Cast<DataRow>()
                .Select(r => r.ItemArray.ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();
        }

        public bool IsScalar { get; }

        public IImmutableList<ColumnSpecification> Columns { get; }

        public IImmutableList<IImmutableList<object>> Data { get; }

        public DataTable? ToDataTable()
        {
            var table = new DataTable();

            foreach (var column in Columns)
            {
                table.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
            }
            foreach (var dataRow in Data)
            {
                table.Rows.Add(dataRow.ToArray());
            }

            return table;
        }

    }
}