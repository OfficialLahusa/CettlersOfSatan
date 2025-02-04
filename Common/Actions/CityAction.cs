using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class CityAction : Action, IActionProvider
    {
        public int IntersectionIndex { get; init; }

        public CityAction(int playerIdx, int intersectionIndex)
            : base(playerIdx)
        {
            IntersectionIndex = intersectionIndex;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Place road
            Intersection intersection = state.Board.Intersections[IntersectionIndex];
            intersection.Owner = PlayerIndex;
            intersection.Building = Intersection.BuildingType.City;

            // Remove cards
            CardSet playerCards = state.Players[PlayerIndex].CardSet;
            playerCards.Remove(CardSet.CardType.Grain, 2);
            playerCards.Remove(CardSet.CardType.Ore, 3);

            // Return cards to bank
            state.Bank.Add(CardSet.CardType.Grain, 2);
            state.Bank.Add(CardSet.CardType.Ore, 3);

            // Remove piece from stock
            state.Players[PlayerIndex].BuildingStock.RemainingCities--;

            // Add previous piece to stock
            state.Players[PlayerIndex].BuildingStock.RemainingSettlements++;
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && !turn.MustMoveRobber;
        }

        public override bool IsBoardValid(GameState state)
        {
            Intersection intersection = state.Board.Intersections[IntersectionIndex];

            bool hasSettlement = intersection.Building == Intersection.BuildingType.Settlement;
            bool ownsSettlement = intersection.Owner == PlayerIndex;
            bool canAfford = state.Players[PlayerIndex].CanAffordCity();

            return hasSettlement && ownsSettlement && canAfford;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = new List<Action>();

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                CityAction action = new(state.Turn.PlayerIndex, intersectionIdx);
                if (action.IsValidFor(state))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + $", I{IntersectionIndex}";
        }
    }
}
