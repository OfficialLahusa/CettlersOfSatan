namespace Common
{
    public class Board
    {
        public HexMap<Tile> Map { get; set; }
        public LinkedList<Intersection> Intersections { get; set; }
        public LinkedList<Edge> Edges { get; set; }

        public Board(HexMap<Tile> map, LinkedList<Intersection> intersections, LinkedList<Edge> edges)
        {
            Map = map;
            Intersections = intersections;
            Edges = edges;
        }
    }
}
