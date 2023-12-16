using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Tile
    {
        public readonly int X, Y;
        public TileType Type { get; set; }
        public int? Number { get; set; }

        public Tile(int x, int y, TileType type, int? number)
        {
            X = x;
            Y = y;
            Type = type;
            Number = number;
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
}
