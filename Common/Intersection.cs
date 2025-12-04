
namespace Common
{
    public class Intersection
    {
        // Key: Corner direction this intersection is on at the given tile
        public SortedList<Direction.Corner, Tile> AdjacentTiles;

        public HashSet<int> AdjacentEdges
        {
            get
            {
                if(_adjacentEdges == null)
                    _adjacentEdges = GetAdjacentEdges();
                return _adjacentEdges;
            }
        }

        public readonly bool FacesDownwards;

        public enum BuildingType : byte
        {
            None = 0,
            Settlement = 1,
            City = 2
        }
        public BuildingType Building { get; set; }

        // -1 => None, 0/1/.. => Player 1/2/..
        public int Owner { get; set; }

        private HashSet<int>? _adjacentEdges = null;

        public Intersection(bool facesDownwards)
        {
            AdjacentTiles = new SortedList<Direction.Corner, Tile>();
            FacesDownwards = facesDownwards;
            Building = BuildingType.None;
            Owner = -1;
        }

        private HashSet<int> GetAdjacentEdges()
        {
            HashSet<int> result = new HashSet<int>();

            foreach ((Direction.Corner anchoredDir, Tile tile) in AdjacentTiles)
            {
                // Get the two roads adjacent to the tile and intersection
                (Direction.Tile leftAdjEdgeDir, Direction.Tile rightAdjEdgeDir) = anchoredDir.GetAdjacentTiles();
                int leftAdjEdge = tile.Edges[leftAdjEdgeDir];
                int rightAdjEdge = tile.Edges[rightAdjEdgeDir];

                result.Add(leftAdjEdge);
                result.Add(rightAdjEdge);
            }

            return result;
        }

        public override bool Equals(object? obj)
        {
            return obj is Intersection intersection &&
                   FacesDownwards == intersection.FacesDownwards &&
                   Building == intersection.Building &&
                   Owner == intersection.Owner;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FacesDownwards, Building, Owner);
        }
    }
}
