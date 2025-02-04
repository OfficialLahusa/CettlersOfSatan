namespace Common
{
    public class Intersection
    {
        // Key: Corner direction this intersection is on at the given tile
        public SortedList<Direction.Corner, Tile> AdjacentTiles;

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

        public Intersection(bool facesDownwards)
        {
            AdjacentTiles = new SortedList<Direction.Corner, Tile>();
            FacesDownwards = facesDownwards;
            Building = BuildingType.None;
            Owner = -1;
        }

        public SortedList<Direction.Edge, Edge> GetAdjacentRoads()
        {
            SortedList<Direction.Edge, Edge> result = new SortedList<Direction.Edge, Edge>();

            foreach ((Direction.Corner anchoredDir, Tile tile) in AdjacentTiles)
            {
                // Get the two roads adjacent to the tile and intersection
                (Direction.Tile leftAdjRoadDir, Direction.Tile rightAdjRoadDir) = anchoredDir.GetAdjacentTiles();
                Edge leftAdjRoad = tile.Edges[leftAdjRoadDir];
                Edge rightAdjRoad = tile.Edges[rightAdjRoadDir];

                if (!result.ContainsKey(leftAdjRoad.Direction))  result.Add(leftAdjRoad.Direction,  leftAdjRoad);
                if (!result.ContainsKey(rightAdjRoad.Direction)) result.Add(rightAdjRoad.Direction, rightAdjRoad);
            }

            return result;
        }
    }
}
