using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class GameState
    {
        public GameSettings Settings { get; set; }
        public TurnState Turn { get; set; }
        public Board Board { get; set; }
        public CardSet Bank { get; set; }
        public PlayerState[] Players { get; set; }

        public GameState(Board board, uint playerCount)
        {
            Settings = new GameSettings();
            Turn = new TurnState();
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
                        uint bankedAmount = Bank.Get(tile.Type.ToCardType());
                        uint awardedAmount = bankedAmount < yieldCount ? bankedAmount : yieldCount;

                        if (awardedAmount == 0) continue;

                        // TODO: If stock is empty for more than one player, nobody gets yields from that resource
                        // Approach: Complete yield summary first, then award based on summary

                        if(tile != Board.Robber)
                        {
                            Bank.Remove(tile.Type.ToCardType(), awardedAmount);
                            Players[intersection.Owner].CardSet.Add(tile.Type.ToCardType(), awardedAmount);
                            yieldSummary[intersection.Owner, Array.IndexOf(CardSet.RESOURCE_CARD_TYPES, tile.Type.ToCardType())] += awardedAmount;
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

        public void ResetCards()
        {
            Bank = CardSet.CreateBank();

            foreach(PlayerState player in Players)
            {
                player.CardSet.Clear();
            }
        }

        public void Reset()
        {
            Turn = new TurnState();
            Bank = CardSet.CreateBank();
            Players = new PlayerState[Players.Length];

            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new PlayerState();
            }
        }
    }
}
