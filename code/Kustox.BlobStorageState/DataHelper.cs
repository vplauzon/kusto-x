using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.BlobStorageState
{
    internal static class DataHelper
    {
        public static T? ArgMaxBy<T, U>(this IEnumerable<T> list, Func<T, U> keySelector)
            where U : IComparable<U>
        {
            T? maxValue = default(T?);
            U? maxKey = default(U?);

            foreach (var item in list)
            {
                var key = keySelector(item);

                if (maxValue == null || key.CompareTo(maxKey) > 0)
                {
                    maxKey = key;
                    maxValue = item;
                }
            }

            return maxValue;
        }
    }
}