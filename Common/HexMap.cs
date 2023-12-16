using System.Numerics;

namespace Common
{
    public class HexMap<T>
    {
        public enum Direction
        {
            West,
            NorthWest,
            NorthEast,
            East,
            SouthEast,
            SouthWest
        }

        private T[] _values;

        // Width of the HexMap (X)
        public uint Width { get; set; }

        // Height of the HexMap (Y)
        public uint Height { get; set; }

        public int Length { get { return _values.Length; } }

        public HexMap(uint width, uint height, T defaultValue = default)
        {
            Width = width;
            Height = height;
            _values = new T[Width*Height];
            Array.Fill<T>(_values, defaultValue);
        }

        // Get a Single Tile
        public T GetTile(int x, int y)
        {
            if (x >= Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            if (y >= Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");
            return _values[y*Width+x];
        }

        // Update a Single Tile 
        public void SetTile(int x, int y, T value)
        {
            if (x >= Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            if (y >= Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");
            // Set Internally Stored Value
            _values[y * Width + x] = value;
        }

        // Update All Tiles from an Array of Values
        public void SetAllTiles(T[,] values)
        {
            if (values.GetLength(0) != Height || values.GetLength(1) != Width) throw new ArgumentOutOfRangeException("Array dimensions must match Width and Height");
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Set Internally Stored Value
                    _values[y*Width+x] = values[x, y];
                }
            }
        }

        // Update All Tiles from an Array of Values
        public void SetAllTiles(T[] values)
        {
            if (values.Length != Width * Height) throw new ArgumentOutOfRangeException("Array length must match Width*Height");
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Set Internally Stored Value
                    _values[y * Width + x] = values[y * Width + x];
                }
            }
        }

        public HashSet<T> Where(Func<T, bool> filter)
        {
            return _values.Where(filter).ToHashSet();
        }

        public HashSet<T> GetNeighbors(int x, int y, Func<T, bool>? filter = null)
        {
            HashSet<T> neighbors = new HashSet<T>();

            // Get neighbor from each direction
            foreach(Direction dir in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                (int nx, int ny) = ShiftPosition(x, y, dir);
                if (Contains(nx, ny))
                {
                    neighbors.Add(GetTile(nx, ny));
                }
            }

            // Return only tiles that match the filter
            if(filter != null)
            {
                return neighbors.Where(filter).ToHashSet();
            }
            return neighbors;
        }

        public Dictionary<Direction, T> GetNeighborsByDirection(int x, int y)
        {
            Dictionary<Direction, T> neighbors = new Dictionary<Direction, T>();

            // Get neighbor from each direction
            foreach (Direction dir in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                (int nx, int ny) = ShiftPosition(x, y, dir);
                if (Contains(nx, ny))
                {
                    neighbors.Add(dir, GetTile(nx, ny));
                }
            }

            return neighbors;
        }

        public bool Contains(int x, int y)
        {
            return x < Width && x >= 0 && y < Height && y >= 0;
        }

        public bool Contains(Vector3 pos)
        {
            (int x, int y) = CubeToEvenR(pos);
            return Contains(x, y);
        }

        /*
         * Distance metric for even-r offset hex map
         * https://stackoverflow.com/a/72385439
         * https://www.redblobgames.com/grids/hexagons/
         */
        public static int Distance(int ax, int ay, int bx, int by)
        {
            Vector3 p1 = EvenRToCube(ax, ay);
            Vector3 p2 = EvenRToCube(bx, by);
            int a = (int)Math.Abs(p1.X - p2.X);
            int b = (int)Math.Abs(p1.Y - p2.Y);
            int c = (int)Math.Abs(p1.Z - p2.Z);
            return Math.Max(a, Math.Max(b, c));
        }

        public static Vector3 ShiftPosition(Vector3 cubePos, Direction dir)
        {
            return cubePos += DirectionToCubeOffset(dir);
        }

        public static (int, int) ShiftPosition(int x, int y, Direction dir)
        {
            return CubeToEvenR(ShiftPosition(EvenRToCube(x, y), dir));
        }

        // https://www.redblobgames.com/grids/hexagons/
        public static Vector3 EvenRToCube(int x, int y)
        {
            int q = x - ((y + (y % 2)) / 2);
            int r = y;
            return new Vector3(q, r, -q - r);
        }

        // https://www.redblobgames.com/grids/hexagons/
        public static (int, int) CubeToEvenR(Vector3 cubePos)
        {
            int x = (int)cubePos.X + ((int)cubePos.Y + ((int)cubePos.Y & 1)) / 2;
            int y = (int)cubePos.Y;
            return (x, y);
        }

        public static Vector3 DirectionToCubeOffset(Direction dir)
        {
            return dir switch
            {
                Direction.West => new Vector3(-1, 0, 1),
                Direction.NorthWest => new Vector3(0, -1, 1),
                Direction.NorthEast => new Vector3(1, -1, 0),
                Direction.East => new Vector3(1, 0, -1),
                Direction.SouthEast => new Vector3(0, 1, -1),
                Direction.SouthWest => new Vector3(-1, 1, 0),
                _ => new Vector3()
            };
        }
    }
}

