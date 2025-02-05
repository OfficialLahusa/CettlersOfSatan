using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class VictoryPointTracker
    {
        public byte SettlementPoints;
        public byte CityPoints;
        public byte DevelopmentCardPoints;
        public byte LongestRoadPoints;
        public byte LargestArmyPoints;

        public int Total => SettlementPoints + CityPoints + DevelopmentCardPoints + LongestRoadPoints + LargestArmyPoints;

        public VictoryPointTracker()
        {
            SettlementPoints = 0;
            CityPoints = 0;
            DevelopmentCardPoints = 0;
            LongestRoadPoints = 0;
            LargestArmyPoints = 0;
        }
    }
}
