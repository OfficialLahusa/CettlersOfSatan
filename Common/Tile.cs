namespace Common
{
    public class Tile
    {
        public readonly int X, Y;
        public enum TileType
        {
            NonPlayable = -1,
            Water,
            Lumber,
            Brick,
            Wool,
            Grain,
            Ore,
            Desert
        };
        public TileType Type { get; set; }
        public int? Number { get; set; }
        public SortedList<Direction.Tile, Tile> Neighbors;
        public SortedList<Direction.Corner, Intersection> Intersections;
        public SortedList<Direction.Tile, Edge> Edges;

        public Tile(int x, int y, TileType type, int? number)
        {
            X = x;
            Y = y;
            Type = type;
            Number = number;
            Neighbors = new SortedList<Direction.Tile, Tile>();
            Intersections = new SortedList<Direction.Corner, Intersection>();
            Edges = new SortedList<Direction.Tile, Edge>();
        }

        public bool IsLandTile()
        {
            return Type != TileType.NonPlayable && Type != TileType.Water;
        }

        public bool HasYield()
        {
            return IsLandTile() && Type != TileType.Desert && Number != null;
        }
    }
}
