using System.Runtime.CompilerServices;
using static Common.CardSet;
using static Common.Tile;

namespace Common
{
    public class Tile
    {
        public readonly int X, Y;
        public enum TileType : byte
        {
            Water,
            Lumber,
            Brick,
            Wool,
            Grain,
            Ore,
            Desert,
            NonPlayable = 255,
        };
        public TileType Type { get; set; }
        public int? Number { get; set; }
        public int YieldPoints
        {
            get
            {
                return Number switch
                {
                    2 or 12 => 1,
                    3 or 11 => 2,
                    4 or 10 => 3,
                    5 or 9 => 4,
                    6 or 8 => 5,
                    _ => 0
                };
            }
        }

        public Port? Port { get; set; }
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

    public static class TileTypeExtensions
    {
        public static CardType ToCardType(this TileType tileType)
        {
            return tileType switch
            {
                TileType.Lumber => CardType.Lumber,
                TileType.Brick => CardType.Brick,
                TileType.Wool => CardType.Wool,
                TileType.Grain => CardType.Grain,
                TileType.Ore => CardType.Ore,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
