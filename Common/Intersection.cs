
namespace Common
{
    public class Intersection
    {
        public readonly byte Index;
        public readonly bool FacesDownwards;

        public enum BuildingType : byte
        {
            None = 0,
            Settlement = 1,
            City = 2
        }
        public BuildingType Building { get; set; }

        // -1 => None, 0/1/.. => Player 1/2/..
        public sbyte Owner { get; set; }

        public Intersection(byte index, bool facesDownwards)
        {
            Index = index;
            FacesDownwards = facesDownwards;
            Building = BuildingType.None;
            Owner = -1;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="copy">Instance to copy</param>
        public Intersection(Intersection copy)
        {
            Index = copy.Index;
            FacesDownwards = copy.FacesDownwards;
            Building = copy.Building;
            Owner = copy.Owner;
        }

        public override bool Equals(object? obj)
        {
            return obj is Intersection intersection &&
                   Index == intersection.Index &&
                   FacesDownwards == intersection.FacesDownwards &&
                   Building == intersection.Building &&
                   Owner == intersection.Owner;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, FacesDownwards, Building, Owner);
        }
    }
}
