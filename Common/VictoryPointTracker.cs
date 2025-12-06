using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Common
{
    public class VictoryPointTracker
    {
        public byte SettlementPoints;
        public byte CityPoints;
        public byte DevelopmentCardPoints;
        public byte LongestRoadPoints;
        public byte LargestArmyPoints;

        [YamlIgnore]
        public int Total => SettlementPoints + CityPoints + DevelopmentCardPoints + LongestRoadPoints + LargestArmyPoints;

        public VictoryPointTracker()
        {
            SettlementPoints = 0;
            CityPoints = 0;
            DevelopmentCardPoints = 0;
            LongestRoadPoints = 0;
            LargestArmyPoints = 0;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="copy">Instance to copy</param>
        public VictoryPointTracker(VictoryPointTracker copy)
        {
            SettlementPoints = copy.SettlementPoints;
            CityPoints = copy.CityPoints;
            DevelopmentCardPoints = copy.DevelopmentCardPoints;
            LongestRoadPoints = copy.LongestRoadPoints;
            LargestArmyPoints = copy.LargestArmyPoints;
        }

        public override bool Equals(object? obj)
        {
            return obj is VictoryPointTracker tracker &&
                   SettlementPoints == tracker.SettlementPoints &&
                   CityPoints == tracker.CityPoints &&
                   DevelopmentCardPoints == tracker.DevelopmentCardPoints &&
                   LongestRoadPoints == tracker.LongestRoadPoints &&
                   LargestArmyPoints == tracker.LargestArmyPoints;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SettlementPoints, CityPoints, DevelopmentCardPoints, LongestRoadPoints, LargestArmyPoints);
        }
    }
}
