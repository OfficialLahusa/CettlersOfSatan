using System.Numerics;

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
        public enum Edge
        {
            UpDown,
            Left,
            Right
        }

        // Pregenerated array of tile direction offsets in cube coordinates
        private static Vector3[] _offsets;
        private static Edge[] _tileToEdge;

        static Direction() 
        {
            _offsets = new Vector3[6]
            {
                new Vector3(-1, 0, 1), // West
                new Vector3(0, -1, 1), // NorthWest
                new Vector3(1, -1, 0), // NorthEast
                new Vector3(1, 0, -1), // East
                new Vector3(0, 1, -1), // SouthEast
                new Vector3(-1, 1, 0)  // SouthWest
            };

            _tileToEdge = new Edge[6]
            {
                Edge.Left,   // SouthWest (down-facing intersection)
                Edge.Left,   // NorthWest (up-facing   intersection)
                Edge.UpDown, // North     (down-facing intersection)
                Edge.Right,  // NorthEast (up-facing   intersection)
                Edge.Right,  // SouthEast (down-facing intersection)
                Edge.UpDown  // South     (up-facing   intersection)
            };
        }

        // Return left and right corners of any given tile direction
        public static (Corner left, Corner right) GetAdjacentCorners(this Tile tile)
        {
            return ((Corner)tile, (Corner)tile.Rotate(1));
        }

        public static (Tile left, Tile right) GetAdjacentTiles(this Corner corner)
        {
            return ((Tile)corner.Rotate(-1), (Tile)corner);
        }

        public static Corner Rotate(this Corner corner, int r)
        {
            return (Corner)Utils.Mod((int)corner + r, 6);
        }

        public static Tile Rotate(this Tile tile, int r)
        {
            return (Tile)Utils.Mod((int)tile + r, 6);
        }

        public static Corner Mirror(this Corner corner)
        {
            return corner.Rotate(3);
        }

        public static Tile Mirror(this Tile tile)
        {
            return tile.Rotate(3);
        }

        public static Vector3 ToCubeOffset(this Tile tile)
        {
            return _offsets[(int)tile];
        }

        public static float ToAngle(this Corner corner)
        {
            return 150.0f + (int)corner * 60.0f;
        }

        // Converts a corner direction into the edge direction that faces it from the intersection at the corner
        // Since there are up- and down-facing intersections, the 6 corner directions are mapped to 3 edge directions
        public static Edge ToInverseEdgeDir(this Corner corner)
        {
            return _tileToEdge[(int)corner.Mirror()];
        }

        // Determines whether the intersection at the corner of a hex faces downwards (otherwise it is upward-facing)
        public static bool HasDownwardsFacingIntersection(this Corner corner)
        {
            return corner == Corner.NorthWest || corner == Corner.NorthEast || corner == Corner.South;
        }
    }
}
