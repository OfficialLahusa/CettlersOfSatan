using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Port
    {
        // Tile the port is situated in
        public Tile AnchorTile { get; set; }

        // Facing direction inside the tile
        public Direction.Tile AnchorDirection { get; set; }

        public enum TradeType : byte
        {
            Generic, // 3:1 
            Lumber,  // 2:1
            Brick,   // 2:1
            Wool,    // 2:1
            Grain,   // 2:1
            Ore,     // 2:1
        };

        public TradeType Type { get; set; }

        public Port(Tile tile, Direction.Tile dir, TradeType type)
        {
            AnchorTile = tile;
            AnchorDirection = dir;
            Type = type;
        }

        public override bool Equals(object? obj)
        {
            return obj is Port port
                // Avoid circular reference checking by only checking X and Y of AnchorTiles
                && AnchorTile.X == port.AnchorTile.X
                && AnchorTile.Y == port.AnchorTile.Y
                && AnchorDirection == port.AnchorDirection 
                && Type == port.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AnchorTile.X, AnchorTile.Y, AnchorDirection, Type);
        }
    }
}
