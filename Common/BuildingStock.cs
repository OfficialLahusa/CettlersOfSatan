using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BuildingStock
    {
        public static readonly uint MAX_ROADS = 15;
        public static readonly uint MAX_SETTLEMENTS = 5;
        public static readonly uint MAX_CITIES = 4;

        public uint RemainingRoads;
        public uint RemainingSettlements;
        public uint RemainingCities;

        // Awarded through road building dev card
        public uint FreeRoads;

        public BuildingStock()
        {
            RemainingRoads = MAX_ROADS;
            RemainingSettlements = MAX_SETTLEMENTS;
            RemainingCities = MAX_CITIES;

            FreeRoads = 0;
        }
    }
}
