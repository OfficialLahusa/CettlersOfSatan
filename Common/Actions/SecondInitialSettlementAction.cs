﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class SecondInitialSettlementAction : Action, IReplayAction, IActionProvider
    {
        public record SecondInitialSettlementActionHistory(ReadOnlyCollection<ResourceCardType> InitialYields);

        public SecondInitialSettlementActionHistory? History { get; private set; }

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
            List<ResourceCardType> awardedResources = [];
            foreach (Tile adjTile in intersection.AdjacentTiles.Values)
            {
                if(adjTile != null && adjTile.HasYield())
                {
                    ResourceCardType adjResource = adjTile.Type.ToCardType();

                    state.Players[PlayerIndex].ResourceCards.Add(adjResource, 1);
                    state.ResourceBank.Remove(adjResource, 1);

                    awardedResources.Add(adjResource);
                }
            }

            History = new SecondInitialSettlementActionHistory(awardedResources.AsReadOnly());

            // Award VP
            state.Players[PlayerIndex].VictoryPoints.SettlementPoints++;

            // Update port privileges
            SettlementAction.UpdatePortPrivileges(state, IntersectionIndex, PlayerIndex);
        }

        public override void Revert(GameState state)
        {
            // Ensure action was applied before
            if (!HasHistory()) throw new InvalidOperationException();

            // Remove settlement
            Intersection intersection = state.Board.Intersections[IntersectionIndex];
            intersection.Owner = -1;
            intersection.Building = Intersection.BuildingType.None;

            // Return piece to stock
            state.Players[PlayerIndex].BuildingStock.RemainingSettlements++;

            // Remove initial resources
            foreach (ResourceCardType resourceType in History!.InitialYields)
            {
                state.Players[PlayerIndex].ResourceCards.Remove(resourceType, 1);
                state.ResourceBank.Add(resourceType, 1);
            }

            // Remove VP
            state.Players[PlayerIndex].VictoryPoints.SettlementPoints--;

            // Recalculate port privileges
            SettlementAction.FullyRecalculatePortPrivileges(state);
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.SecondInitial;
        }

        public bool IsBoardValid(GameState state)
        {
            Intersection intersection = state.Board.Intersections[IntersectionIndex];

            bool spaceFree = intersection.Building == Intersection.BuildingType.None;
            bool oneSettlementPlaced = state.Players[PlayerIndex].BuildingStock.RemainingSettlements == BuildingStock.MAX_SETTLEMENTS - 1;
            bool oneRoadPlaced = state.Players[PlayerIndex].BuildingStock.RemainingRoads == BuildingStock.MAX_ROADS - 1;

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

            return spaceFree && oneSettlementPlaced && oneRoadPlaced && !hasAdjacentSettlement;
        }

        public bool HasHistory()
        {
            return History != null;
        }

        public void ClearHistory()
        {
            History = null;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = new List<Action>();

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                SecondInitialSettlementAction action = new(playerIdx, intersectionIdx);
                if (action.IsBoardValid(state))
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
