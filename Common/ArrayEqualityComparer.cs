using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ArrayEqualityComparer<T> : IEqualityComparer<T[]?>
    {
        public bool Equals(T[]? a, T[]? b)
        {
            if (a == null && b == null)
                return true;

            if (a == null && b != null || a != null && b == null)
                return false;

            return a!.SequenceEqual(b!);
        }

        public int GetHashCode(T[] arr)
        {
            if (arr == null) return 0;
            return arr.Aggregate(0, (acc, v) => HashCode.Combine(acc, v!.GetHashCode()));
        }
    }
}
