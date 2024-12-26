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
        public PlayerState[] Players { get; set; }

        public GameState(Board board, uint playerCount)
        {
            Board = board;
            Bank = CardSet.CreateBank();
            Players = new PlayerState[playerCount];

            for(int i = 0; i < playerCount; i++)
            {
                Players[i] = new PlayerState();
            }
        }

        public (uint[,] yieldSummary, uint robbedYields) AwardYields(int number)
        {
            uint[,] yieldSummary = new uint[Players.Length, CardSet.RESOURCE_CARD_TYPES.Length];
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
                            Players[intersection.Owner].CardSet.Add(tile.Type.ToCardType(), yieldCount);
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
            bool canAfford = Players[playerIdx].CanAffordRoad();

            // TODO: Check available spaces

            return canAfford;
        }

        public bool CanBuildSettlement(int playerIdx)
        {
            bool canAfford = Players[playerIdx].CanAffordSettlement();

            // TODO: Check available spaces

            return canAfford;
        }

        public bool CanBuildCity(int playerIdx)
        {
            bool canAfford = Players[playerIdx].CanAffordCity();

            // TODO: Check available spaces

            return canAfford;
        }

        public bool CanBuyDevelopmentCard(int playerIdx)
        {
            bool canAfford = Players[playerIdx].CanAffordDevelopmentCard();
            bool bankHasCard = Bank.GetDevelopmentCardCount() > 0;

            return canAfford && bankHasCard;
        }

        public void ResetCards()
        {
            Bank = CardSet.CreateBank();

            foreach(PlayerState player in Players)
            {
                player.CardSet.Clear();
            }
        }
    }
}
