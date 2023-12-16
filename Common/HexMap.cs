using System.Numerics;

namespace Common
{
    public class HexMap<T>
    {
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
            Vector3 cubePos = EvenRToCube(x, y);
            Vector3[] offsets = new Vector3[]
            {
                new Vector3( 1,  0, -1),
                new Vector3( 1, -1,  0),
                new Vector3( 0, -1,  1),
                new Vector3(-1,  0,  1),
                new Vector3(-1,  1,  0),
                new Vector3( 0,  1, -1)
            };

            foreach(Vector3 offset in offsets)
            {
                Vector3 neighborPos = offset + cubePos;
                (int nx, int ny) = CubeToEvenR(neighborPos);
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
        public static int Distance(int p1x, int p1y, int p2x, int p2y)
        {
            Vector3 point_1 = EvenRToCube(p1x, p1y);
            Vector3 point_2 = EvenRToCube(p2x, p2y);
            int a = (int)Math.Abs(point_1.X - point_2.X);
            int b = (int)Math.Abs(point_1.Y - point_2.Y);
            int c = (int)Math.Abs(point_1.Z - point_2.Z);
            return Math.Max(a, Math.Max(b, c));
        }

        // https://www.redblobgames.com/grids/hexagons/
        public static Vector3 EvenRToCube(int x, int y)
        {
            int q = x - ((y + (y % 2)) / 2);
            int r = y;
            return new Vector3(q, r, -q - r);
        }

        // https://www.redblobgames.com/grids/hexagons/
        public static (int, int) CubeToEvenR(Vector3 cube)
        {
            int x = (int)cube.X + ((int)cube.Y + ((int)cube.Y & 1)) / 2;
            int y = (int)cube.Y;
            return (x, y);
        }
    }
}

