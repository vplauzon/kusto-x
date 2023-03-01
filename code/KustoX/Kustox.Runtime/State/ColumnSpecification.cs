using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public class ColumnSpecification
    {
        public ColumnSpecification(string columnName, string columnTypeName)
        {
            var type = Type.GetType(columnTypeName);
            
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
    }
}