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

        public Intersection Top
        {
            get
            {
                if (_topIntersection == null)
                    (_topIntersection, _bottomIntersection) = GetIntersections();
                return _topIntersection;
            }
        }
        public Intersection Bottom
        {
            get
            {
                if (_bottomIntersection == null)
                    (_topIntersection, _bottomIntersection) = GetIntersections();
                return _bottomIntersection;
            }
        }
        public (Intersection Top, Intersection Bottom) Intersections
        {
            get => (Top, Bottom);
        }

        private Intersection? _topIntersection;
        private Intersection? _bottomIntersection;

        public Edge(Direction.Edge direction)
        {
            WestTile = null;
            EastTile = null;
            Direction = direction;
            Building = BuildingType.None;
            Owner = -1;
        }

        private (Intersection top, Intersection bottom) GetIntersections()
        {
            bool fromWest = WestTile != null;
            Tile anchorTile = WestTile ?? EastTile!;

            Direction.Tile tileDir = fromWest ? Direction.ToEastTileDir() : Direction.ToWestTileDir();
            (Direction.Corner left, Direction.Corner right) = tileDir.GetAdjacentCorners();

            Intersection leftIntersection = anchorTile.Intersections[left];
            Intersection rightIntersection = anchorTile.Intersections[right];

            return (fromWest ? leftIntersection : rightIntersection, fromWest ? rightIntersection : leftIntersection);
        }
    }
}
