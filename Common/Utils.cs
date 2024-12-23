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
    }
}
