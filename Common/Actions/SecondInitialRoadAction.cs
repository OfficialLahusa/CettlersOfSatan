using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class SecondInitialRoadAction : Action, IActionProvider
    {
        public int EdgeIndex { get; init; }

        public SecondInitialRoadAction(int playerIdx, int edgeIndex)
            : base(playerIdx)
        {
            EdgeIndex = edgeIndex;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Place road
            Edge road = state.Board.Edges[EdgeIndex];
            road.Owner = PlayerIndex;
            road.Building = Edge.BuildingType.Road;

            // Remove piece from stock
            state.Players[PlayerIndex].BuildingStock.RemainingRoads--;

            // Update turn state
            if (state.Turn.PlayerIndex == 0)
            {
                state.Turn.RoundCounter++;
                state.Turn.TypeOfRound = TurnState.RoundType.Normal;
                state.Turn.MustRoll = true;
            }
            else
            {
                state.Turn.PlayerIndex--;
                state.Turn.MustRoll = false;
            }
            state.Turn.MustMoveRobber = false;
            state.Turn.AwaitedDiscards = 0;
            state.Turn.HasPlayedDevelopmentCard = false;

            state.CalculateLongestRoad(PlayerIndex);
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.SecondInitial;
        }

        public override bool IsBoardValid(GameState state)
        {
            Edge road = state.Board.Edges[EdgeIndex];

            bool spaceFree = road.Building == Edge.BuildingType.None;
            bool twoSettlementsPlaced = state.Players[PlayerIndex].BuildingStock.RemainingSettlements == BuildingStock.MAX_SETTLEMENTS - 2;
            bool oneRoadPlaced = state.Players[PlayerIndex].BuildingStock.RemainingRoads == BuildingStock.MAX_ROADS - 1;

            // Get the two intersections on the ends of the road
            (Intersection top, Intersection bottom) = road.GetIntersections();

            // Check if one of the intersections has a building owned by the player
            bool hasTopBuilding = top.Owner == PlayerIndex && top.Building != Intersection.BuildingType.None;
            bool hasBottomBuilding = bottom.Owner == PlayerIndex && bottom.Building != Intersection.BuildingType.None;

            bool hasDirectAdjBuilding = hasTopBuilding || hasBottomBuilding;

            bool isAdjacentToFirstSettlement = false;
            if(hasDirectAdjBuilding)
            {
                foreach (Edge adjEdge in (hasTopBuilding ? top : bottom).GetAdjacentRoads().Values)
                {
                    if(adjEdge.Owner == PlayerIndex && adjEdge.Building != Edge.BuildingType.None)
                    {
                        isAdjacentToFirstSettlement = true;
                        break;
                    }
                }
            }

            bool isAdjacentToSecondSettlement = hasDirectAdjBuilding && !isAdjacentToFirstSettlement;

            return spaceFree && twoSettlementsPlaced && oneRoadPlaced && isAdjacentToSecondSettlement;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            for (int edgeIdx = 0; edgeIdx < state.Board.Edges.Count; edgeIdx++)
            {
                SecondInitialRoadAction action = new(state.Turn.PlayerIndex, edgeIdx);

                if (action.IsValidFor(state))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + $", I{EdgeIndex}";
        }
    }
}
