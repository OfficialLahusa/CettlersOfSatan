namespace Common
{
    public class Board
    {
        public HexMap<Tile> Map { get; set; }
        public LinkedList<Intersection> Intersections { get; set; }
        public LinkedList<Edge> Edges { get; set; }
        public LinkedList<Port> Ports { get; set; }

        public Board(HexMap<Tile> map, LinkedList<Intersection> intersections, LinkedList<Edge> edges, LinkedList<Port> ports)
        {
            Map = map;
            Intersections = intersections;
            Edges = edges;
            Ports = ports;
        }
    }
}
