using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Tile
    {
        public TileType Type { get; set; }
        public int? Number { get; set; }

        public Tile(TileType type, int? number)
        {
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

        public static Tile Empty = new Tile(TileType.NonPlayable, null);
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
