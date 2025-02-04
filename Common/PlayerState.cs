using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.CardSet;

namespace Common
{
    public class PlayerState
    {
        public CardSet CardSet;
        // Index: Development Card Type Index (Knight = 0)
        public uint[] NewDevelopmentCards;

        public BuildingStock BuildingStock;
        public PortPrivileges PortPrivileges;

        public PlayerState()
        {
            CardSet = new CardSet();
            NewDevelopmentCards = new uint[DEVELOPMENT_CARD_TYPES.Length];

            BuildingStock = new BuildingStock();
            PortPrivileges = PortPrivileges.None;
        }

        public bool CanAffordRoad()
        {
            bool hasFreeRoad = BuildingStock.FreeRoads > 0;
            bool canBuyNormally = BuildingStock.RemainingRoads > 0 && CardSet.Contains(CardType.Lumber) && CardSet.Contains(CardType.Brick);
            return hasFreeRoad || canBuyNormally;
        }

        public bool CanAffordSettlement()
        {
            return BuildingStock.RemainingSettlements > 0 
                && CardSet.Contains(CardType.Lumber) && CardSet.Contains(CardType.Brick) 
                && CardSet.Contains(CardType.Wool) && CardSet.Contains(CardType.Grain);
        }

        public bool CanAffordCity()
        {
            return BuildingStock.RemainingCities > 0 && CardSet.Contains(CardType.Grain, 2) && CardSet.Contains(CardType.Ore, 3);
        }

        public bool CanAffordDevelopmentCard()
        {
            return CardSet.Contains(CardType.Wool) && CardSet.Contains(CardType.Grain) && CardSet.Contains(CardType.Ore);
        }
    }


}
