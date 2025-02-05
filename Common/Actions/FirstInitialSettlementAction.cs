using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class FirstInitialSettlementAction : Action, IActionProvider
    {
        public int IntersectionIndex { get; init; }

        public FirstInitialSettlementAction(int playerIdx, int intersectionIndex)
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

            // Remove piece from stock
            state.Players[PlayerIndex].BuildingStock.RemainingSettlements--;

            // Award VP
            state.Players[PlayerIndex].VictoryPoints.SettlementPoints++;
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.FirstInitial;
        }

        public override bool IsBoardValid(GameState state)
        {
            Intersection intersection = state.Board.Intersections[IntersectionIndex];

            bool spaceFree = intersection.Building == Intersection.BuildingType.None;
            bool noSettlementPlaced = state.Players[PlayerIndex].BuildingStock.RemainingSettlements == BuildingStock.MAX_SETTLEMENTS;

            // No adjacent settlements
            bool hasAdjacentSettlement = false;
            foreach (Edge adjacentRoad in intersection.AdjacentEdges.Values)
            {
                (Intersection top, Intersection bottom) = adjacentRoad.Intersections;
                if (top.Building != Intersection.BuildingType.None || bottom.Building != Intersection.BuildingType.None)
                {
                    hasAdjacentSettlement = true;
                }
            }

            return spaceFree && noSettlementPlaced && !hasAdjacentSettlement;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = new List<Action>();

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                FirstInitialSettlementAction action = new(state.Turn.PlayerIndex, intersectionIdx);
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
