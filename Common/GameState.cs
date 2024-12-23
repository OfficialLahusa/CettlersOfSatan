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

        public GameState(Board board, uint playerCount)
        {
            Board = board;
            Bank = CardSet.CreateBank();
            PlayerCards = new CardSet[playerCount];

            for(int i = 0; i < playerCount; i++)
            {
                PlayerCards[i] = new CardSet();
            }
        }

        public uint[,] AwardYields(int number)
        {
            uint[,] yieldSummary = new uint[PlayerCards.Length, CardSet.RESOURCE_CARD_TYPES.Length];

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

                        PlayerCards[intersection.Owner].Add(tile.Type.ToCardType(), yieldCount);

                        yieldSummary[intersection.Owner, Array.IndexOf(CardSet.RESOURCE_CARD_TYPES, tile.Type.ToCardType())] += yieldCount;
                    }
                }
            }

            return yieldSummary;
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
