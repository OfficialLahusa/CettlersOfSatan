using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Edge
    {
        public byte Index { get; init; }
        public Direction.Edge Direction { get; init; }

        public enum BuildingType : byte
        {
            None = 0,
            Road = 1,
            // Ship = 2 (For future use)
        }
        public BuildingType Building { get; set; }

        // -1 => None, 0/1/.. => Player 1/2/..
        public sbyte Owner { get; set; }

        public Edge(byte index, Direction.Edge direction)
        {
            Index = index;
            Direction = direction;
            Building = BuildingType.None;
            Owner = -1;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="copy">Instance to copy</param>
        public Edge(Edge copy)
        {
            Index = copy.Index;
            Direction = copy.Direction;
            Building = copy.Building;
            Owner = copy.Owner;
        }

        /// <summary>
        /// Parameterless constructor for deserialization
        /// </summary>
        private Edge()
        {

        }

        public override bool Equals(object? obj)
        {
            return obj is Edge edge
                && Index == edge.Index
                && Direction == edge.Direction 
                && Building == edge.Building 
                && Owner == edge.Owner;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Direction, Building, Owner);
        }
    }
}
