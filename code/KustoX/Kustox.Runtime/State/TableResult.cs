using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public bool IsScalar { get; }

        public IImmutableList<ColumnSpecification> Columns { get; }

        public IImmutableList<IImmutableList<object>> Data { get; }
    }
}