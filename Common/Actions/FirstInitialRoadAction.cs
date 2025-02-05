using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class FirstInitialRoadAction : Action, IActionProvider
    {
        public int EdgeIndex { get; init; }

        public FirstInitialRoadAction(int playerIdx, int edgeIndex)
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
            if (state.Turn.PlayerIndex == state.Players.Length - 1)
            {
                state.Turn.RoundCounter++;
                state.Turn.TypeOfRound = TurnState.RoundType.SecondInitial;
            }
            else
            {
                state.Turn.PlayerIndex++;
            }
            state.Turn.MustRoll = false;
            state.Turn.MustMoveRobber = false;
            state.Turn.AwaitedDiscards = 0;
            state.Turn.HasPlayedDevelopmentCard = false;

            state.CalculateLongestRoad(PlayerIndex);
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.FirstInitial;
        }

        public override bool IsBoardValid(GameState state)
        {
            Edge road = state.Board.Edges[EdgeIndex];

            bool spaceFree = road.Building == Edge.BuildingType.None;
            bool oneSettlementPlaced = state.Players[PlayerIndex].BuildingStock.RemainingSettlements == BuildingStock.MAX_SETTLEMENTS - 1;
            bool noRoadPlaced = state.Players[PlayerIndex].BuildingStock.RemainingRoads == BuildingStock.MAX_ROADS;

            // Get the two intersections on the ends of the road
            (Intersection top, Intersection bottom) = road.GetIntersections();

            // Check if one of the intersections has a building owned by the player
            bool hasTopBuilding = top.Owner == PlayerIndex && top.Building != Intersection.BuildingType.None;
            bool hasBottomBuilding = bottom.Owner == PlayerIndex && bottom.Building != Intersection.BuildingType.None;

            bool hasDirectAdjBuilding = hasTopBuilding || hasBottomBuilding;

            return spaceFree && oneSettlementPlaced && noRoadPlaced && hasDirectAdjBuilding;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            for (int edgeIdx = 0; edgeIdx < state.Board.Edges.Count; edgeIdx++)
            {
                FirstInitialRoadAction action = new(state.Turn.PlayerIndex, edgeIdx);

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
