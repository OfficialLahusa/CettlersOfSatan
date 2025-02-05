namespace Common
{
    public class Intersection
    {
        // Key: Corner direction this intersection is on at the given tile
        public SortedList<Direction.Corner, Tile> AdjacentTiles;

        public SortedList<Direction.Edge, Edge> AdjacentEdges
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

        private SortedList<Direction.Edge, Edge>? _adjacentEdges = null;

        public Intersection(bool facesDownwards)
        {
            AdjacentTiles = new SortedList<Direction.Corner, Tile>();
            FacesDownwards = facesDownwards;
            Building = BuildingType.None;
            Owner = -1;
        }

        private SortedList<Direction.Edge, Edge> GetAdjacentEdges()
        {
            SortedList<Direction.Edge, Edge> result = new SortedList<Direction.Edge, Edge>();

            foreach ((Direction.Corner anchoredDir, Tile tile) in AdjacentTiles)
            {
                // Get the two roads adjacent to the tile and intersection
                (Direction.Tile leftAdjEdgeDir, Direction.Tile rightAdjEdgeDir) = anchoredDir.GetAdjacentTiles();
                Edge leftAdjEdge = tile.Edges[leftAdjEdgeDir];
                Edge rightAdjEdge = tile.Edges[rightAdjEdgeDir];

                if (!result.ContainsKey(leftAdjEdge.Direction))  result.Add(leftAdjEdge.Direction,  leftAdjEdge);
                if (!result.ContainsKey(rightAdjEdge.Direction)) result.Add(rightAdjEdge.Direction, rightAdjEdge);
            }

            return result;
        }
    }
}
