using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
