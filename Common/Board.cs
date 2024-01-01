namespace Common
{
    public class Board
    {
        public HexMap<Tile> Map { get; set; }
        public LinkedList<Intersection> Intersections { get; set; }

        public Board(HexMap<Tile> map, LinkedList<Intersection> intersections)
        {
            Map = map;
            Intersections = intersections;
        }
    }
}
