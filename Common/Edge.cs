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

        public enum BuildingType : byte
        {
            None = 0,
            Road = 1,
            // Ship = 2 (For future use)
        }
        public BuildingType Building { get; set; }

        // -1 => None, 0/1/.. => Player 1/2/..
        public int Owner { get; set; }

        public int Top
        {
            get
            {
                if (!_topIntersection.HasValue)
                    (_topIntersection, _bottomIntersection) = GetIntersections();
                return _topIntersection.Value;
            }
        }
        public int Bottom
        {
            get
            {
                if (!_bottomIntersection.HasValue)
                    (_topIntersection, _bottomIntersection) = GetIntersections();
                return _bottomIntersection.Value;
            }
        }
        public (int Top, int Bottom) Intersections
        {
            get => (Top, Bottom);
        }

        private int? _topIntersection;
        private int? _bottomIntersection;

        public Edge(Direction.Edge direction)
        {
            WestTile = null;
            EastTile = null;
            Direction = direction;
            Building = BuildingType.None;
            Owner = -1;
        }

        private (int top, int bottom) GetIntersections()
        {
            bool fromWest = WestTile != null;
            Tile anchorTile = WestTile ?? EastTile!;

            Direction.Tile tileDir = fromWest ? Direction.ToEastTileDir() : Direction.ToWestTileDir();
            (Direction.Corner left, Direction.Corner right) = tileDir.GetAdjacentCorners();

            int leftIntersection = anchorTile.Intersections[left];
            int rightIntersection = anchorTile.Intersections[right];

            return (fromWest ? leftIntersection : rightIntersection, fromWest ? rightIntersection : leftIntersection);
        }

        public override bool Equals(object? obj)
        {
            return obj is Edge edge
                && Tile.Equals(WestTile, edge.WestTile) 
                && Tile.Equals(EastTile, edge.EastTile)
                && Direction == edge.Direction 
                && Building == edge.Building 
                && Owner == edge.Owner;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WestTile, EastTile, Direction, Building, Owner);
        }
    }
}
