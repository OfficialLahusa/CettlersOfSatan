using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class RobberAction : Action, IActionProvider
    {
        public int TargetTileIndex { get; init; }
        public int? TargetPlayerIndex { get; init; }

        public RobberAction(int playerIdx, int targetTileIdx, int? targetPlayerIdx)
            : base(playerIdx)
        {
            TargetTileIndex = targetTileIdx;
            TargetPlayerIndex = targetPlayerIdx;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            Tile tile = state.Board.Map.ElementAt(TargetTileIndex);
            state.Board.Robber = tile;

            // Draw from adjacent player
            if (TargetPlayerIndex.HasValue)
            {
                CardSet.CardType? drawnCard = state.Players[TargetPlayerIndex.Value].CardSet.DrawByType(CardSet.RESOURCE_CARD_TYPES, true);
                if (drawnCard.HasValue)
                {
                    state.Players[PlayerIndex].CardSet.Add(drawnCard.Value, 1);
                }
            }

            // Update turn state
            state.Turn.MustMoveRobber = false;
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && turn.MustMoveRobber;
        }

        public override bool IsBoardValid(GameState state)
        {
            Tile tile = state.Board.Map.ElementAt(TargetTileIndex);

            bool isLandTile = tile.IsLandTile();
            bool alreadyHasRobber = state.Board.Robber == tile;

            if (!isLandTile || alreadyHasRobber) return false;

            // Target player specification
            bool hasValidTarget;

            if(TargetPlayerIndex.HasValue)
            {
                bool isTargetPlayerAdjacent = false;

                foreach (Intersection intersection in tile.Intersections.Values)
                {
                    if (intersection.Owner == TargetPlayerIndex && intersection.Building != Intersection.BuildingType.None)
                    {
                        isTargetPlayerAdjacent = true;
                        break;
                    }
                }

                hasValidTarget = TargetPlayerIndex.Value != PlayerIndex && isTargetPlayerAdjacent;
            }
            else
            {
                bool hasOtherAdjacentPlayer = false;

                foreach (Intersection intersection in tile.Intersections.Values)
                {
                    if (intersection.Owner != PlayerIndex && intersection.Building != Intersection.BuildingType.None)
                    {
                        hasOtherAdjacentPlayer = true;
                        break;
                    }
                }

                hasValidTarget = !hasOtherAdjacentPlayer;
            }

            return hasValidTarget;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            //if(!state.Turn.MustMoveRobber) return actions;

            for(int tileIdx = 0; tileIdx < state.Board.Map.Length; tileIdx++)
            {
                for(int targetPlayerIdx = -1; targetPlayerIdx < state.Players.Length; targetPlayerIdx++)
                {
                    RobberAction action = new(state.Turn.PlayerIndex, tileIdx, targetPlayerIdx < 0 ? null : targetPlayerIdx);

                    if(action.IsValidFor(state))
                    {
                        actions.Add(action);
                    }
                }
            }

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + $", T{TargetTileIndex}" + (TargetPlayerIndex.HasValue ? $", D{TargetPlayerIndex}" : "");
        }
    }
}
