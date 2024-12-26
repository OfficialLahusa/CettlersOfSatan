namespace Common
{
    public class Board
    {
        public HexMap<Tile> Map { get; set; }
        public List<Intersection> Intersections { get; set; }
        public List<Edge> Edges { get; set; }
        public List<Port> Ports { get; set; }
        public Tile? Robber;

        public Board(HexMap<Tile> map, List<Intersection> intersections, List<Edge> edges, List<Port> ports, Tile? robber)
        {
            Map = map;
            Intersections = intersections;
            Edges = edges;
            Ports = ports;
            Robber = robber;
        }
    }
}
