using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public class ColumnSpecification
    {
        public ColumnSpecification(string columnName, string columnTypeName)
        {
            var type = columnTypeName == "System.Data.SqlTypes.SqlDecimal"
                ? typeof(SqlDecimal)
                : Type.GetType(columnTypeName);

            ColumnName = columnName;
            if (type == null)
            {
                throw new NotSupportedException(
                    $"Unsupported column type:  '{columnTypeName}'");
            }
            ColumnType = type;
        }

        public ColumnSpecification(string columnName, Type columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }

        public string ColumnName { get; }

        public Type ColumnType { get; }

        public string GetKustoType()
        {
            if (ColumnType == typeof(sbyte))
            {
                return "bool";
            }
            else if (ColumnType == typeof(DateTime))
            {
                return "datetime";
            }
            else if (ColumnType == typeof(object))
            {
                return "dynamic";
            }
            else if (ColumnType == typeof(Guid))
            {
                return "guid";
            }
            else if (ColumnType == typeof(int))
            {
                return "int";
            }
            else if (ColumnType == typeof(long))
            {
                return "long";
            }
            else if (ColumnType == typeof(double))
            {
                return "real";
            }
            else if (ColumnType == typeof(string))
            {
                return "string";
            }
            else if (ColumnType == typeof(TimeSpan))
            {
                return "timespan";
            }
            else if (ColumnType == typeof(SqlDecimal))
            {
                return "decimal";
            }
            else
            {
                throw new NotSupportedException($".NET Type:  {ColumnType.FullName}");
            }
        }
    }
}