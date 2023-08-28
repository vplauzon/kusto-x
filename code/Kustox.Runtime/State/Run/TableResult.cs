using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kustox.Runtime.State.Run
{
    public class TableResult
    {
        public TableResult(
            bool isScalar,
            IImmutableList<ColumnSpecification> columns,
            IImmutableList<IImmutableList<object>> data)
        {
            IsScalar = isScalar;
            if (isScalar)
            {
                Columns = columns.Take(1).ToImmutableArray();
                Data = data
                    .Take(1)
                    .Select(r => r.Take(1))
                    .Cast<IImmutableList<object>>()
                    .ToImmutableArray();
            }
            else
            {
                Columns = columns;
                Data = data;
            }
            if (Columns.Count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }
            if (data.Select(row => row.Count()).Any(l => l != Columns.Count()))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(data),
                    "Some row(s) don't have the right column count");
            }
        }

        public bool IsScalar { get; }

        public IImmutableList<ColumnSpecification> Columns { get; }

        public IImmutableList<IImmutableList<object>> Data { get; }

        public TableResult ToScale()
        {
            return new TableResult(true, Columns, Data);
        }

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

        public string GetJsonData()
        {
            return JsonSerializer.Serialize(Data);
        }

        public IEnumerable<object> GetColumnData(int columnIndex)
        {
            foreach (var array in Data)
            {
                yield return array[columnIndex];
            }
        }

        public TableResult AlignDataWithNativeTypes()
        {
            var alignedData = Data
                .Select(row => row.Select(item => AlignWithNativeTypes(item)).ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();

            return new TableResult(IsScalar, Columns, alignedData);
        }

        public static TableResult Union(IImmutableList<TableResult> results)
        {
            if (!results.Any())
            {
                throw new ArgumentOutOfRangeException(
                    nameof(results),
                    $"Union requires at list one {typeof(TableResult).Name}");
            }
            if (results.Any(r => r == null))
            {
                throw new ArgumentNullException(
                    nameof(results),
                    $"Some or all {typeof(TableResult).Name} are null");
            }

            var schemas = results.Select(r => r.Columns);
            var template = schemas.First();

            foreach (var schema in schemas)
            {
                if (!Enumerable.SequenceEqual(schema, template))
                {
                    throw new InvalidDataException(
                        "Not all results have the same schema for union");
                }
            }

            var datas = new Stack<IEnumerable<IImmutableList<object>>>(
                results.Select(r => r.Data).Reverse());

            while (datas.Count() > 1)
            {
                var first = datas.Pop();
                var second = datas.Pop();
                var union = first.Concat(second);

                datas.Push(union);
            }

            var allUnion = datas.Pop().ToImmutableArray();

            return new TableResult(false, template, allUnion);
        }

        private static object AlignWithNativeTypes(object item)
        {
            if (item is IEnumerable enumerable)
            {
                var newArray = enumerable
                    .Cast<object>()
                    .Select(subItem => AlignWithNativeTypes(subItem))
                    .ToImmutableArray();

                return newArray;
            }
            else if(item is JsonElement element)
            {
                switch(element.ValueKind)
                {
                    case JsonValueKind.Array:
                        return AlignWithNativeTypes(element.EnumerateArray().ToImmutableArray());
                    default:
                        return item;
                }
            }
            else
            {
                return item;
            }
        }
    }
}