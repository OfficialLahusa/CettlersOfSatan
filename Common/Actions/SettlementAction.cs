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
            CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;
            playerCards.Remove(ResourceCardType.Lumber, 1);
            playerCards.Remove(ResourceCardType.Brick, 1);
            playerCards.Remove(ResourceCardType.Wool, 1);
            playerCards.Remove(ResourceCardType.Grain, 1);

            // Return cards to bank
            state.ResourceBank.Add(ResourceCardType.Lumber, 1);
            state.ResourceBank.Add(ResourceCardType.Brick, 1);
            state.ResourceBank.Add(ResourceCardType.Wool, 1);
            state.ResourceBank.Add(ResourceCardType.Grain, 1);

            // Remove piece from stock
            state.Players[PlayerIndex].BuildingStock.RemainingSettlements--;

            // Award VP
            state.Players[PlayerIndex].VictoryPoints.SettlementPoints++;

            // Update port privileges
            UpdatePortPrivileges(state, IntersectionIndex, PlayerIndex);

            // Check if the settlement might have broken the longest road, which can only happen if there are two adjacent roads from another player
            int? adjPlayer = null;
            foreach (Edge adjRoad in intersection.AdjacentEdges.Values)
            {
                if (adjRoad.Building == Edge.BuildingType.None || adjRoad.Owner == PlayerIndex) continue;

                // First road from another player
                if (adjPlayer == null)
                {
                    adjPlayer = adjRoad.Owner;
                }
                // Second road from another player
                else if (adjPlayer == adjRoad.Owner)
                {
                    // Recalculate longest road and award VPs accordingly, since it can be broken here
                    state.CalculateLongestRoad(adjRoad.Owner, true);
                }
            }

            // Check for match completion
            state.CheckForCompletion();
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
            Intersection intersection = state.Board.Intersections[IntersectionIndex];

            // Can afford
            bool spaceFree = intersection.Building == Intersection.BuildingType.None;
            bool canAfford = state.Players[PlayerIndex].CanAffordSettlement();

            // Has adjacent road
            bool hasAdjacentRoad = false;
            bool hasAdjacentSettlement = false;
            foreach(Edge adjacentRoad in intersection.AdjacentEdges.Values)
            {
                if (adjacentRoad.Owner == PlayerIndex && adjacentRoad.Building != Edge.BuildingType.None)
                {
                    hasAdjacentRoad = true;
                }

                (Intersection top, Intersection bottom) = adjacentRoad.Intersections;
                if (top.Building != Intersection.BuildingType.None || bottom.Building != Intersection.BuildingType.None)
                {
                    hasAdjacentSettlement = true;
                }
            }

            return spaceFree && canAfford && hasAdjacentRoad && !hasAdjacentSettlement;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = new List<Action>();

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                SettlementAction action = new(playerIdx, intersectionIdx);
                if (action.IsBoardValid(state))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        public static void UpdatePortPrivileges(GameState state, int intersectionIdx, int playerIdx)
        {
            foreach (Port port in state.Board.Ports)
            {
                Edge portEdge = port.AnchorTile.Neighbors[port.AnchorDirection].Edges[port.AnchorDirection.Mirror()];
                Intersection settlementIntersection = state.Board.Intersections[intersectionIdx];

                // On port
                if (settlementIntersection == portEdge.Top || settlementIntersection == portEdge.Bottom)
                {
                    state.Players[playerIdx].PortPrivileges |= port.Type switch
                    {
                        Port.TradeType.Generic => PortPrivileges.GenericThreeToOne,
                        Port.TradeType.Lumber => PortPrivileges.LumberTwoToOne,
                        Port.TradeType.Brick => PortPrivileges.BrickTwoToOne,
                        Port.TradeType.Wool => PortPrivileges.WoolTwoToOne,
                        Port.TradeType.Grain => PortPrivileges.GrainTwoToOne,
                        Port.TradeType.Ore => PortPrivileges.OreTwoToOne,
                        _ => throw new NotImplementedException(),
                    };

                    break;
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + $", I{IntersectionIndex}";
        }
    }
}
