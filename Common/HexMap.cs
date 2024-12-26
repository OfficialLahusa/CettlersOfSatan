using System.Collections;
using System.Numerics;

namespace Common
{
    public class HexMap<T> : IEnumerable<T>
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
            Array.Fill(_values, defaultValue);
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
            foreach(Direction.Tile dir in (Direction.Tile[])Enum.GetValues(typeof(Direction.Tile)))
            {
                (int nx, int ny) = Coordinates.Shift(x, y, dir);
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

        public SortedList<Direction.Tile, T> GetNeighborsByDirection(int x, int y, Func<T, bool>? filter = null)
        {
            SortedList<Direction.Tile, T> neighbors = new SortedList<Direction.Tile, T>();

            // Get neighbor from each direction
            foreach (Direction.Tile dir in (Direction.Tile[])Enum.GetValues(typeof(Direction.Tile)))
            {
                (int nx, int ny) = Coordinates.Shift(x, y, dir);
                if (Contains(nx, ny))
                {
                    neighbors.Add(dir, GetTile(nx, ny));
                }
            }

            // Return only tiles that match the filter
            if (filter != null)
            {
                return new SortedList<Direction.Tile, T>(neighbors.Where(kvp => filter(kvp.Value)).ToDictionary(e => e.Key, e => e.Value));
            }
            return neighbors;
        }

        public bool Contains(int x, int y)
        {
            return x < Width && x >= 0 && y < Height && y >= 0;
        }

        public bool Contains(Vector3 pos)
        {
            (int x, int y) = Coordinates.CubeToEvenR(pos);
            return Contains(x, y);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
    }
}

