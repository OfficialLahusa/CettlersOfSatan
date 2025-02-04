using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BuildingStock
    {
        public uint RemainingRoads;
        public uint RemainingSettlements;
        public uint RemainingCities;

        // Awarded through road building dev card
        public uint FreeRoads;

        public BuildingStock()
        {
            RemainingRoads = 15;
            RemainingSettlements = 5;
            RemainingCities = 4;

            FreeRoads = 0;
        }
    }
}
