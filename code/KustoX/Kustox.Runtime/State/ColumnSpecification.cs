using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public class ColumnSpecification
    {
        public ColumnSpecification(string columnName, string columnType)
        {
            ColumnName = columnName;
            ColumnType = columnType;
        }

        public string ColumnName { get; }

        public string ColumnType { get; }
    }
}