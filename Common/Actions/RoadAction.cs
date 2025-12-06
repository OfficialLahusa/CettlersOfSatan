using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class RoadAction : Action, IReplayAction, IActionProvider
    {
        public record RoadActionHistory(int PrevLongestRoadHolder, bool WasFree);

        public RoadActionHistory? History { get; private set; }

        public int EdgeIndex { get; init; }

        public RoadAction(sbyte playerIdx, int edgeIndex)
            : base(playerIdx)
        {
            EdgeIndex = edgeIndex;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Determine previous longest road holder
            int currentLongestRoadHolder = -1;

            for (int playerIdx = 0; playerIdx < state.Players.Length; playerIdx++)
            {
                if (state.Players[playerIdx].VictoryPoints.LongestRoadPoints > 0)
                {
                    currentLongestRoadHolder = playerIdx;
                    break;
                }
            }

            // Place road
            Edge road = state.Board.Edges[EdgeIndex];
            road.Owner = PlayerIndex;
            road.Building = Edge.BuildingType.Road;

            // Remove cards and/or building stock
            BuildingStock buildingStock = state.Players[PlayerIndex].BuildingStock;
            bool hasFreeStock = buildingStock.FreeRoads > 0;

            History = new RoadActionHistory(currentLongestRoadHolder, hasFreeStock);

            // Use free road building dev card stock first
            if (hasFreeStock)
            {
                buildingStock.FreeRoads--;
            }
            // Otherwise, buy normally
            else
            {
                // Remove cards
                CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;
                playerCards.Remove(ResourceCardType.Lumber, 1);
                playerCards.Remove(ResourceCardType.Brick, 1);

                // Return cards to bank
                state.ResourceBank.Add(ResourceCardType.Lumber, 1);
                state.ResourceBank.Add(ResourceCardType.Brick, 1);

                // Remove piece from stock
                buildingStock.RemainingRoads--;
            }

            // Recalculate longest road and award VPs accordingly
            state.CalculateLongestRoad(PlayerIndex);

            // Check for match completion
            state.CheckForCompletion();
        }

        public override void Revert(GameState state)
        {
            // Ensure action was applied before
            if (!HasHistory()) throw new InvalidOperationException();

            // Remove road
            Edge road = state.Board.Edges[EdgeIndex];
            road.Owner = -1;
            road.Building = Edge.BuildingType.None;

            // Return cards and/or building stock
            BuildingStock buildingStock = state.Players[PlayerIndex].BuildingStock;

            // Return free building stock
            if (History!.WasFree)
            {
                buildingStock.FreeRoads++;
            }
            // Otherwise, return normal cost
            else
            {
                // Return cards
                CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;
                playerCards.Add(ResourceCardType.Lumber, 1);
                playerCards.Add(ResourceCardType.Brick, 1);

                // Remove cards from bank
                state.ResourceBank.Remove(ResourceCardType.Lumber, 1);
                state.ResourceBank.Remove(ResourceCardType.Brick, 1);

                // Return piece to stock
                buildingStock.RemainingRoads++;
            }

            // Recalculate longest road
            state.CalculateLongestRoad(PlayerIndex);

            // Move VPs to previous longest road holder in case of draws (or nobody if -1)
            for (int playerIdx = 0; playerIdx < state.Players.Length; playerIdx++)
            {
                state.Players[playerIdx].VictoryPoints.LongestRoadPoints = (byte)(playerIdx == History.PrevLongestRoadHolder ? 2 : 0);
            }

            // Un-complete match
            state.Turn.TypeOfRound = TurnState.RoundType.Normal;
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && !turn.MustMoveRobber;
        }

        public bool IsBoardValid(GameState state)
        {
            Edge road = state.Board.Edges[EdgeIndex];

            bool spaceFree = road.Building == Edge.BuildingType.None;
            bool canAfford = state.Players[PlayerIndex].CanAffordRoad();

            // Get the two intersections on the ends of the road
            (Intersection top, Intersection bottom) = state.Board.Adjacency.GetIntersections(road);

            // Check if one of the intersections has a building owned by the player
            bool hasTopBuilding = top.Owner == PlayerIndex && top.Building != Intersection.BuildingType.None;
            bool hasBottomBuilding = bottom.Owner == PlayerIndex && bottom.Building != Intersection.BuildingType.None;

            bool hasDirectAdjBuilding = hasTopBuilding || hasBottomBuilding;

            // Check if any of the intersections connect to roads of the player
            bool topBlocked = top.Owner != PlayerIndex && top.Building != Intersection.BuildingType.None;
            bool bottomBlocked = bottom.Owner != PlayerIndex && bottom.Building != Intersection.BuildingType.None;

            bool topHasAdjRoad = false;
            bool bottomHasAdjRoad = false;

            // Check if one of the adjacent roads of the top intersection was built by the player
            foreach (Edge adjacentRoad in state.Board.Adjacency.GetEdges(top))
            {
                if (adjacentRoad.Owner == PlayerIndex && adjacentRoad.Building != Edge.BuildingType.None)
                {
                    topHasAdjRoad = true;
                    break;
                }
            }

            // Check if one of the adjacent roads of the bottom intersection was built by the player
            foreach (Edge adjacentRoad in state.Board.Adjacency.GetEdges(bottom))
            {
                if (adjacentRoad.Owner == PlayerIndex && adjacentRoad.Building != Edge.BuildingType.None)
                {
                    bottomHasAdjRoad = true;
                    break;
                }
            }

            bool hasDirectAdjRoad = (topHasAdjRoad && !topBlocked) || (bottomHasAdjRoad && !bottomBlocked);

            return spaceFree && canAfford && (hasDirectAdjBuilding || hasDirectAdjRoad);
        }

        public bool HasHistory()
        {
            return History != null;
        }

        public void ClearHistory()
        {
            History = null;
        }

        public static List<Action> GetActionsForState(GameState state, sbyte playerIdx)
        {
            List<Action> actions = [];

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            for(int edgeIdx = 0; edgeIdx < state.Board.Edges.Count; edgeIdx++)
            {
                RoadAction action = new(playerIdx, edgeIdx);

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
