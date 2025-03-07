﻿
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

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="copy">Instance to copy</param>
        public Board(Board copy)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            // Reset buildings
            foreach(Intersection intersection in Intersections)
            {
                intersection.Owner = -1;
                intersection.Building = Intersection.BuildingType.None;
            }

            foreach(Edge edge in Edges)
            {
                edge.Owner = -1;
                edge.Building = Edge.BuildingType.None;
            }

            // Reset robber
            foreach(Tile tile in Map)
            {
                if (tile.Type == Tile.TileType.Desert)
                {
                    Robber = tile;
                    break;
                }
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Board board
                && Map.Equals(board.Map)
                && Intersections.SequenceEqual(board.Intersections)
                && Edges.SequenceEqual(board.Edges)
                && Ports.SequenceEqual(board.Ports)
                && Tile.Equals(Robber, board.Robber);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Map, Intersections, Edges, Ports, Robber);
        }
    }
}
