using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class SecondInitialSettlementAction : Action, IActionProvider
    {
        public int IntersectionIndex { get; init; }

        public SecondInitialSettlementAction(int playerIdx, int intersectionIndex)
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

            // Award adjacent resources
            foreach (Tile adjTile in intersection.AdjacentTiles.Values)
            {
                if(adjTile != null && adjTile.HasYield())
                {
                    state.Players[PlayerIndex].CardSet.Add(adjTile.Type.ToCardType(), 1);
                    state.Bank.Remove(adjTile.Type.ToCardType(), 1);
                }
            }
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.SecondInitial;
        }

        public override bool IsBoardValid(GameState state)
        {
            Intersection intersection = state.Board.Intersections[IntersectionIndex];

            bool spaceFree = intersection.Building == Intersection.BuildingType.None;
            bool oneSettlementPlaced = state.Players[PlayerIndex].BuildingStock.RemainingSettlements == BuildingStock.MAX_SETTLEMENTS - 1;
            bool oneRoadPlaced = state.Players[PlayerIndex].BuildingStock.RemainingRoads == BuildingStock.MAX_ROADS - 1;

            // No adjacent settlements
            bool hasAdjacentSettlement = false;
            foreach (Edge adjacentRoad in intersection.GetAdjacentRoads().Values)
            {
                (Intersection top, Intersection bottom) = adjacentRoad.GetIntersections();
                if (top.Building != Intersection.BuildingType.None || bottom.Building != Intersection.BuildingType.None)
                {
                    hasAdjacentSettlement = true;
                }
            }

            return spaceFree && oneSettlementPlaced && oneRoadPlaced && !hasAdjacentSettlement;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = new List<Action>();

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                SecondInitialSettlementAction action = new(state.Turn.PlayerIndex, intersectionIdx);
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
