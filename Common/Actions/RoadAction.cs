using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class RoadAction : Action, IActionProvider
    {
        public int EdgeIndex { get; init; }

        public RoadAction(int playerIdx, int edgeIndex)
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

            // Remove cards and/or building stock
            BuildingStock buildingStock = state.Players[PlayerIndex].BuildingStock;

            // Use free road building dev card stock first
            if (buildingStock.FreeRoads > 0)
            {
                buildingStock.FreeRoads--;
            }
            // Otherwise, buy normally
            else
            {
                // Remove cards
                CardSet playerCards = state.Players[PlayerIndex].CardSet;
                playerCards.Remove(CardSet.CardType.Lumber, 1);
                playerCards.Remove(CardSet.CardType.Brick, 1);

                // Return cards to bank
                state.Bank.Add(CardSet.CardType.Lumber, 1);
                state.Bank.Add(CardSet.CardType.Brick, 1);

                // Remove piece from stock
                buildingStock.RemainingRoads--;
            }

            // Recalculate longest road and award VPs accordingly
            state.CalculateLongestRoad(PlayerIndex);

            // Check for match completion
            state.CheckForCompletion();
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
            Edge road = state.Board.Edges[EdgeIndex];

            bool spaceFree = road.Building == Edge.BuildingType.None;
            bool canAfford = state.Players[PlayerIndex].CanAffordRoad();

            // Get the two intersections on the ends of the road
            (Intersection top, Intersection bottom) = road.Intersections;

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
            foreach(Edge adjacentRoad in top.AdjacentEdges.Values)
            {
                if (adjacentRoad.Owner == PlayerIndex && adjacentRoad.Building != Edge.BuildingType.None)
                {
                    topHasAdjRoad = true;
                    break;
                }
            }

            // Check if one of the adjacent roads of the bottom intersection was built by the player
            foreach (Edge adjacentRoad in bottom.AdjacentEdges.Values)
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

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            for(int edgeIdx = 0; edgeIdx < state.Board.Edges.Count; edgeIdx++)
            {
                RoadAction action = new(state.Turn.PlayerIndex, edgeIdx);

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
