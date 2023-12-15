using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public static class Util
    {
        public static Random random;

        static Util()
        {
            random = new Random();
        }

        public static Color Vec3ToColor(System.Numerics.Vector3 color)
        {
            return new Color((byte)(255 * color.X), (byte)(255 * color.Y), (byte)(255 * color.Z));
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = list.Count; i > 0; i--)
                list.Swap(0, random.Next(0, i));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
