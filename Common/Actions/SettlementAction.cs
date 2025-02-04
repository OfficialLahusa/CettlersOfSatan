using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class SettlementAction : Action, IActionProvider
    {
        public int IntersectionIndex { get; init; }

        public SettlementAction(int playerIdx, int intersectionIndex)
            : base(playerIdx)
        {
            IntersectionIndex = intersectionIndex;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Place settlement
            Intersection intersection = state.Board.Intersections[IntersectionIndex];
            intersection.Owner = PlayerIndex;
            intersection.Building = Intersection.BuildingType.Settlement;

            // Remove cards
            CardSet playerCards = state.Players[PlayerIndex].CardSet;
            playerCards.Remove(CardSet.CardType.Lumber, 1);
            playerCards.Remove(CardSet.CardType.Brick, 1);
            playerCards.Remove(CardSet.CardType.Wool, 1);
            playerCards.Remove(CardSet.CardType.Grain, 1);

            // Return cards to bank
            state.Bank.Add(CardSet.CardType.Lumber, 1);
            state.Bank.Add(CardSet.CardType.Brick, 1);
            state.Bank.Add(CardSet.CardType.Wool, 1);
            state.Bank.Add(CardSet.CardType.Grain, 1);

            // Remove piece from stock
            state.Players[PlayerIndex].BuildingStock.RemainingSettlements--;

            // TODO: Recalculate longest road and award VPs accordingly, since it can be broken here
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

            // Can afford
            bool spaceFree = intersection.Building == Intersection.BuildingType.None;
            bool canAfford = state.Players[PlayerIndex].CanAffordSettlement();

            // Has adjacent road
            bool hasAdjacentRoad = false;
            bool hasAdjacentSettlement = false;
            foreach(Edge adjacentRoad in intersection.GetAdjacentRoads().Values)
            {
                if (adjacentRoad.Owner == PlayerIndex && adjacentRoad.Building != Edge.BuildingType.None)
                {
                    hasAdjacentRoad = true;
                }

                (Intersection top, Intersection bottom) = adjacentRoad.GetIntersections();
                if (top.Building != Intersection.BuildingType.None || bottom.Building != Intersection.BuildingType.None)
                {
                    hasAdjacentSettlement = true;
                }
            }

            return spaceFree && canAfford && hasAdjacentRoad && !hasAdjacentSettlement;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = new List<Action>();

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                SettlementAction action = new(state.Turn.PlayerIndex, intersectionIdx);
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
