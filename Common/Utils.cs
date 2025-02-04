using System.Collections.Immutable;
using static Common.CardSet;
using static Common.Tile;

namespace Common
{
    public static class Utils
    {
        public static Random Random;

        static Utils()
        {
            Random = new Random();
        }

        // Fisher-Yates Shuffle
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                Swap(list, k, n);
            }
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        // Actual modulo operation, instead of C#'s remainder (%)
        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static List<T[]> GetSubsets<T>(ReadOnlySpan<T> collection, int k)
        {
            // Precalculate capacity to avoid resizing
            List<T[]> result = new List<T[]>(GetBinCoeff(collection.Length, k));
            GetSubsetsRecursive(result, collection.Length, k, 0, collection, []);
            return result;
        }

        private static void GetSubsetsRecursive<T>(List<T[]> subsets, int n, int k, int i, ReadOnlySpan<T> collection, T[] current_subset)
        {
            if (n < k) return;
            if (k == 0)
            {
                subsets.Add([.. current_subset]);
                return;
            }

            GetSubsetsRecursive(subsets, n - 1, k, i + 1, collection, current_subset);
            GetSubsetsRecursive(subsets, n - 1, k - 1, i + 1, collection, [.. current_subset, collection[i]]);
        }

        public static int GetBinCoeff(int N, int K)
        {
            // This function gets the total number of unique combinations based upon N and K.
            // N is the total number of items.
            // K is the size of the group.
            // Total number of unique combinations = N! / ( K! (N - K)! ).
            // This function is less efficient, but is more likely to not overflow when N and K are large.
            // Taken from:  http://blog.plover.com/math/choose.html
            //
            int r = 1;
            int d;
            if (K > N) return 0;
            for (d = 1; d <= K; d++)
            {
                r *= N--;
                r /= d;
            }
            return r;
        }
    }
}
