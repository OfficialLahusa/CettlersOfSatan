using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Coordinates
    {
        /*
         * Distance metric for even-r offset hex map
         * https://stackoverflow.com/a/72385439
         * https://www.redblobgames.com/grids/hexagons/
         */
        public static int Distance(int ax, int ay, int bx, int by)
        {
            Vector3 p1 = EvenRToCube(ax, ay);
            Vector3 p2 = EvenRToCube(bx, by);
            return Distance(p1, p2);
        }

        // Cube coordinate Manhattan distance
        public static int Distance(Vector3 p1, Vector3 p2)
        {
            int a = (int)Math.Abs(p1.X - p2.X);
            int b = (int)Math.Abs(p1.Y - p2.Y);
            int c = (int)Math.Abs(p1.Z - p2.Z);
            return Math.Max(a, Math.Max(b, c));
        }

        public static Vector3 Shift(Vector3 cubePos, Direction.Tile dir)
        {
            return cubePos += Direction.ToCubeOffset(dir);
        }

        public static (int x, int y) Shift(int x, int y, Direction.Tile dir)
        {
            return CubeToEvenR(Shift(EvenRToCube(x, y), dir));
        }

        public static Vector3 EvenRToCube(int x, int y)
        {
            int q = x - ((y + (y % 2)) / 2);
            int r = y;
            return new Vector3(q, r, -q - r);
        }

        public static (int x, int y) CubeToEvenR(Vector3 cubePos)
        {
            int x = (int)cubePos.X + ((int)cubePos.Y + ((int)cubePos.Y & 1)) / 2;
            int y = (int)cubePos.Y;
            return (x, y);
        }
    }
}
