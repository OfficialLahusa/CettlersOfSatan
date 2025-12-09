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

        public FirstInitialRoadAction(sbyte playerIdx, int edgeIndex)
            : base(playerIdx)
        {
            EdgeIndex = edgeIndex;
        }

        /// <summary>
        /// Parameterless constructor for deserialization
        /// </summary>
        private FirstInitialRoadAction()
            : base(-1)
        { }

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

            // TODO: Remove?
            state.Turn.MustRoll = false;
            state.Turn.MustMoveRobber = false;
            Array.Fill(state.Turn.AwaitedPlayerDiscards, false);
            state.Turn.HasPlayedDevelopmentCard = false;

            state.CalculateLongestRoad(PlayerIndex);
        }

        public override void Revert(GameState state)
        {
            // Remove road
            Edge road = state.Board.Edges[EdgeIndex];
            road.Owner = -1;
            road.Building = Edge.BuildingType.None;

            // Return piece to stock
            state.Players[PlayerIndex].BuildingStock.RemainingRoads++;

            // Update turn state
            if (state.Turn.TypeOfRound == TurnState.RoundType.SecondInitial)
            {
                state.Turn.RoundCounter--;
                state.Turn.TypeOfRound = TurnState.RoundType.FirstInitial;
            }
            else
            {
                state.Turn.PlayerIndex--;
            }

            state.CalculateLongestRoad(PlayerIndex);
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.FirstInitial;
        }

        public bool IsBoardValid(GameState state)
        {
            Edge road = state.Board.Edges[EdgeIndex];

            bool spaceFree = road.Building == Edge.BuildingType.None;
            bool oneSettlementPlaced = state.Players[PlayerIndex].BuildingStock.RemainingSettlements == BuildingStock.MAX_SETTLEMENTS - 1;
            bool noRoadPlaced = state.Players[PlayerIndex].BuildingStock.RemainingRoads == BuildingStock.MAX_ROADS;

            // Get the two intersections on the ends of the road
            (Intersection top, Intersection bottom) = state.Board.Adjacency.GetIntersections(road);

            // Check if one of the intersections has a building owned by the player
            bool hasTopBuilding = top.Owner == PlayerIndex && top.Building != Intersection.BuildingType.None;
            bool hasBottomBuilding = bottom.Owner == PlayerIndex && bottom.Building != Intersection.BuildingType.None;

            // TODO: Irgendwo hier ist was off
            bool hasDirectAdjBuilding = hasTopBuilding || hasBottomBuilding;

            return spaceFree && oneSettlementPlaced && noRoadPlaced && hasDirectAdjBuilding;
        }

        public static List<Action> GetActionsForState(GameState state, sbyte playerIdx)
        {
            List<Action> actions = [];

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            for (int edgeIdx = 0; edgeIdx < state.Board.Edges.Count; edgeIdx++)
            {
                FirstInitialRoadAction action = new(playerIdx, edgeIdx);

                if (action.IsBoardValid(state))
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
