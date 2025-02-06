using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class PlayerState
    {
        public VictoryPointTracker VictoryPoints;
        public uint PlayedKnights;
        public uint LongestRoadLength;

        public CardSet<ResourceCardType> ResourceCards;
        public CardSet<DevelopmentCardType> DevelopmentCards;
        public CardSet<DevelopmentCardType> NewDevelopmentCards;

        public BuildingStock BuildingStock;
        public PortPrivileges PortPrivileges;

        public PlayerState()
        {
            VictoryPoints = new VictoryPointTracker();
            PlayedKnights = 0;
            LongestRoadLength = 0;

            ResourceCards = new();
            DevelopmentCards = new();
            NewDevelopmentCards = new();

            BuildingStock = new BuildingStock();
            PortPrivileges = PortPrivileges.None;
        }

        public bool CanAffordRoad()
        {
            bool hasFreeRoad = BuildingStock.FreeRoads > 0;
            bool canBuyNormally = BuildingStock.RemainingRoads > 0 && ResourceCards.Contains(ResourceCardType.Lumber) && ResourceCards.Contains(ResourceCardType.Brick);
            return hasFreeRoad || canBuyNormally;
        }

        public bool CanAffordSettlement()
        {
            return BuildingStock.RemainingSettlements > 0 
                && ResourceCards.Contains(ResourceCardType.Lumber) && ResourceCards.Contains(ResourceCardType.Brick) 
                && ResourceCards.Contains(ResourceCardType.Wool) && ResourceCards.Contains(ResourceCardType.Grain);
        }

        public bool CanAffordCity()
        {
            return BuildingStock.RemainingCities > 0 && ResourceCards.Contains(ResourceCardType.Grain, 2) && ResourceCards.Contains(ResourceCardType.Ore, 3);
        }

        public bool CanAffordDevelopmentCard()
        {
            return ResourceCards.Contains(ResourceCardType.Wool) && ResourceCards.Contains(ResourceCardType.Grain) && ResourceCards.Contains(ResourceCardType.Ore);
        }
    }


}
