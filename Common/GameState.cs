using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class GameState
    {
        public Board Board { get; set; }
        public CardSet Bank { get; set; }
        public CardSet[] PlayerCards { get; set; }
        public BuildingStock[] PlayerStock { get; set; }

        public GameState(Board board, uint playerCount)
        {
            Board = board;
            Bank = CardSet.CreateBank();
            PlayerCards = new CardSet[playerCount];
            PlayerStock = new BuildingStock[playerCount];

            for(int i = 0; i < playerCount; i++)
            {
                PlayerCards[i] = new CardSet();
                PlayerStock[i] = new BuildingStock();
            }
        }

        public (uint[,] yieldSummary, uint robbedYields) AwardYields(int number)
        {
            uint[,] yieldSummary = new uint[PlayerCards.Length, CardSet.RESOURCE_CARD_TYPES.Length];
            uint robbedYields = 0;

            foreach (Tile tile in Board.Map.Where(x => x.HasYield() && x.Number == number))
            {
                foreach(Intersection intersection in tile.Intersections.Values)
                {
                    uint yieldCount = intersection.Building switch
                    {
                        Intersection.BuildingType.City => 2,
                        Intersection.BuildingType.Settlement => 1,
                        _ => 0
                    };

                    if (yieldCount > 0)
                    {
                        // TODO: Subtract and limit bank stock

                        if(tile != Board.Robber)
                        {
                            PlayerCards[intersection.Owner].Add(tile.Type.ToCardType(), yieldCount);
                            yieldSummary[intersection.Owner, Array.IndexOf(CardSet.RESOURCE_CARD_TYPES, tile.Type.ToCardType())] += yieldCount;
                        }
                        else
                        {
                            robbedYields += yieldCount;
                        }
                    }
                }
            }

            return (yieldSummary, robbedYields);
        }

        public bool CanBuildRoad(int playerIdx)
        {
            bool canAfford = PlayerCards[playerIdx].CanAffordRoad();
            bool hasPiece = PlayerStock[playerIdx].RemainingRoads > 0;

            // TODO: Check available spaces

            return canAfford && hasPiece;
        }

        public bool CanBuildSettlement(int playerIdx)
        {
            bool canAfford = PlayerCards[playerIdx].CanAffordSettlement();
            bool hasPiece = PlayerStock[playerIdx].RemainingSettlements > 0;

            // TODO: Check available spaces

            return canAfford && hasPiece;
        }

        public bool CanBuildCity(int playerIdx)
        {
            bool canAfford = PlayerCards[playerIdx].CanAffordCity();
            bool hasPiece = PlayerStock[playerIdx].RemainingCities > 0;

            // TODO: Check available spaces

            return canAfford && hasPiece;
        }

        public bool CanBuyDevelopmentCard(int playerIdx)
        {
            bool canAfford = PlayerCards[playerIdx].CanAffordDevelopmentCard();
            bool bankHasCard = Bank.GetDevelopmentCardCount() > 0;

            return canAfford && bankHasCard;
        }

        public void ResetCards()
        {
            Bank = CardSet.CreateBank();

            foreach(CardSet playerCardSet in PlayerCards)
            {
                playerCardSet.Clear();
            }
        }
    }
}
