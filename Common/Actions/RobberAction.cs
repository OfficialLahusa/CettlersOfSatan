using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Actions.RollAction;

namespace Common.Actions
{
    public class RobberAction : Action, IActionProvider
    {
        public record RobberActionOutcome(ResourceCardType? stolenCard = null);

        public RobberActionOutcome? Outcome { get; private set; }

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

            // Ensure action was not applied before
            if (Outcome != null) throw new InvalidOperationException();

            Tile tile = state.Board.Map.ElementAt(TargetTileIndex);
            state.Board.Robber = tile;

            // Draw from adjacent player
            if (TargetPlayerIndex.HasValue)
            {
                if (state.Players[TargetPlayerIndex.Value].ResourceCards.Count() > 0)
                {
                    ResourceCardType drawnCard = state.Players[TargetPlayerIndex.Value].ResourceCards.Draw(true);
                    state.Players[PlayerIndex].ResourceCards.Add(drawnCard, 1);

                    Outcome = new RobberActionOutcome(drawnCard);
                }
            }

            if (Outcome == null)
            {
                Outcome = new RobberActionOutcome();
            }

            // Update turn state
            state.Turn.MustMoveRobber = false;
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.Normal
                // Robber actions can happen before rolling when a knight is used before the roll
                && !turn.MustDiscard
                && turn.MustMoveRobber;
        }

        public bool IsBoardValid(GameState state)
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

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            if(!IsTurnValid(state.Turn, playerIdx)) return actions;

            for(int tileIdx = 0; tileIdx < state.Board.Map.Length; tileIdx++)
            {
                for(int targetPlayerIdx = -1; targetPlayerIdx < state.Players.Length; targetPlayerIdx++)
                {
                    RobberAction action = new(playerIdx, tileIdx, targetPlayerIdx < 0 ? null : targetPlayerIdx);

                    if(action.IsBoardValid(state))
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
