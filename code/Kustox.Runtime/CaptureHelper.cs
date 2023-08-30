using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal static class CaptureHelper
    {
        public static TableResult? GetCapturedValueIfExist(
            this IImmutableDictionary<string, TableResult?> captures,
            string name)
        {
            if (captures.TryGetValue(name, out var result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}