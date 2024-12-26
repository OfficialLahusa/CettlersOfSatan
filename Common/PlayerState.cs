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
        public BuildingStock BuildingStock;
        public PortPrivileges PortPrivileges;

        public PlayerState()
        {
            CardSet = new CardSet();
            BuildingStock = new BuildingStock();
            PortPrivileges = PortPrivileges.None;
        }

        public bool CanAffordRoad()
        {
            return BuildingStock.RemainingRoads > 0 && CardSet.Contains(CardType.Lumber) && CardSet.Contains(CardType.Brick);
        }

        public bool CanAffordSettlement()
        {
            return BuildingStock.RemainingSettlements > 0 && CanAffordRoad() && CardSet.Contains(CardType.Wool) && CardSet.Contains(CardType.Grain);
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
