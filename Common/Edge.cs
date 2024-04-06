using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Edge
    {
        // Adjacent Tiles
        public Tile? WestTile;
        public Tile? EastTile;

        public readonly Direction.Edge Direction;

        public enum BuildingType
        {
            None = 0,
            Road = 1,
            // Ship = 2 (For future use)
        }
        public BuildingType Building { get; set; }

        // -1 => None, 0/1/.. => Player 1/2/..
        public int Owner { get; set; }

        public Edge(Direction.Edge direction)
        {
            WestTile = null;
            EastTile = null;
            Direction = direction;
            Building = BuildingType.None;
            Owner = -1;
        }
    }
}
