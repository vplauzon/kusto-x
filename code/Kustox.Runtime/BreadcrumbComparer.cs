using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal class BreadcrumbComparer : IEqualityComparer<IImmutableList<int>>
    {
        bool IEqualityComparer<IImmutableList<int>>.Equals(
            IImmutableList<int>? x,
            IImmutableList<int>? y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null || y == null)
            {
                return false;
            }
            else
            {
                return Enumerable.SequenceEqual(x, y);
            }
        }

        int IEqualityComparer<IImmutableList<int>>.GetHashCode(IImmutableList<int> sequence)
        {
            var hash = 0;

            foreach (var item in sequence)
            {
                hash ^= item.GetHashCode();
            }

            return hash;
        }
    }
}