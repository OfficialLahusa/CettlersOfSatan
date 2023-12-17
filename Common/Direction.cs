using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Direction
    {
        public enum Tile
        {
            West = 0,
            NorthWest = 1,
            NorthEast = 2,
            East = 3,
            SouthEast = 4,
            SouthWest = 5
        }
        public enum Corner
        {
            SouthWest = 0,
            NorthWest = 1,
            North = 2,
            NorthEast = 3,
            SouthEast = 4,
            South = 5
        }

        // Return left and right corners of any given tile direction
        public static (Corner left, Corner right) GetAdjacentCorners(Tile dir)
        {
            return ((Corner)dir, (Corner)((int)(dir + 1) % 6));
        }

        public static (Tile left, Tile right) GetAdjacentTiles(Corner corner)
        {
            return ((Tile)((int)(corner - 1) % 6), (Tile)corner);
        }

        public static Vector3 ToCubeOffset(Tile dir)
        {
            return dir switch
            {
                Tile.West => new Vector3(-1, 0, 1),
                Tile.NorthWest => new Vector3(0, -1, 1),
                Tile.NorthEast => new Vector3(1, -1, 0),
                Tile.East => new Vector3(1, 0, -1),
                Tile.SouthEast => new Vector3(0, 1, -1),
                Tile.SouthWest => new Vector3(-1, 1, 0),
                _ => new Vector3()
            };
        }
    }
}
