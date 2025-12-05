using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Port;

namespace Common
{
    public class Port
    {
        // Tile X coordinate the port is situated in
        public int AnchorTileX { get; set; }
        public int AnchorTileY { get; set; }

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

        public Port(int anchorTileX, int anchorTileY, Direction.Tile dir, TradeType type)
        {
            AnchorTileX = anchorTileX;
            AnchorTileY = anchorTileY;
            AnchorDirection = dir;
            Type = type;
        }

        public override bool Equals(object? obj)
        {
            return obj is Port port
                // Avoid circular reference checking by only checking X and Y of AnchorTiles
                && AnchorTileX == port.AnchorTileX
                && AnchorTileY == port.AnchorTileY
                && AnchorDirection == port.AnchorDirection 
                && Type == port.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AnchorTileX, AnchorTileY, AnchorDirection, Type);
        }
    }

    public static class TradeTypeExtensions
    {
        public static PortPrivileges GetPortPrivilege(this TradeType type)
        {
            return type switch
            {
                TradeType.Generic => PortPrivileges.GenericThreeToOne,
                TradeType.Lumber => PortPrivileges.LumberTwoToOne,
                TradeType.Brick => PortPrivileges.BrickTwoToOne,
                TradeType.Wool => PortPrivileges.WoolTwoToOne,
                TradeType.Grain => PortPrivileges.GrainTwoToOne,
                TradeType.Ore => PortPrivileges.OreTwoToOne,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
