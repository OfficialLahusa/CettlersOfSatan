namespace Common
{
    public class Board
    {
        public HexMap<Tile> Map { get; set; }
        public LinkedList<Intersection> Intersections { get; set; }
        public LinkedList<Edge> Edges { get; set; }
        public LinkedList<Port> Ports { get; set; }
        public Tile? Robber;

        public Board(HexMap<Tile> map, LinkedList<Intersection> intersections, LinkedList<Edge> edges, LinkedList<Port> ports, Tile? robber)
        {
            Map = map;
            Intersections = intersections;
            Edges = edges;
            Ports = ports;
            Robber = robber;
        }
    }
}
