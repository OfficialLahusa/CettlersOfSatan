﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class RollAction : Action, IActionProvider
    {
        public record RollActionOutcome(bool TriggeredRobber, uint[,]? AwardedYields = null, uint RobbedYields = 0, uint CappedYields = 0);

        public RollActionOutcome? Outcome { get; private set; }

        public RollResult RollResult { get; init; }

        // Randomize roll when null is passed
        // The option to specify the roll result is primarily used for action tree exploration
        public RollAction(int playerIdx, RollResult? rollResult)
            : base(playerIdx)
        {
            RollResult = rollResult ?? RollResult.GetRandom();
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Ensure action wasn't applied before
            if (Outcome != null) throw new InvalidOperationException();

            bool robberTriggered = RollResult.Total == 7;

            if (robberTriggered)
            {
                // Require discard if card limit is exceeded
                for (int player = 0; player < state.Players.Length; player++)
                {
                    bool mustDiscard = state.Players[player].ResourceCards.Count() > state.Settings.RobberCardLimit;
                    state.Turn.AwaitedPlayerDiscards[player] = mustDiscard;
                }

                // Require robber move
                state.Turn.MustMoveRobber = true;

                Outcome = new RollActionOutcome(robberTriggered);
            }
            else
            {
                (uint[,] yieldSummary, uint robbedYields, uint cappedYields) = AwardYields(state, RollResult.Total);

                Outcome = new RollActionOutcome(robberTriggered, yieldSummary, robbedYields, cappedYields);
            }

            state.Turn.LastRoll = RollResult;
            state.Turn.MustRoll = false;
        }

        private (uint[,] yieldSummary, uint robbedYields, uint cappedYields) AwardYields(GameState state, int number)
        {
            uint[,] yieldSummary = new uint[state.Players.Length, CardSet<ResourceCardType>.Values.Count];
            uint robbedYields = 0;

            // Calculate tile yields
            foreach (Tile tile in state.Board.Map.Where(x => x.HasYield() && x.Number == number))
            {
                foreach (Intersection intersection in tile.Intersections.Values)
                {
                    uint yieldCount = intersection.Building switch
                    {
                        Intersection.BuildingType.City => 2,
                        Intersection.BuildingType.Settlement => 1,
                        _ => 0
                    };

                    if (yieldCount > 0)
                    {
                        if (tile != state.Board.Robber)
                        {
                            yieldSummary[intersection.Owner, CardSet<ResourceCardType>.ToInt(tile.Type.ToCardType())] += yieldCount;
                        }
                        else
                        {
                            robbedYields += yieldCount;
                        }
                    }
                }
            }

            // Award yields according to limited bank stock
            uint cappedYields = 0;
            for (int resourceTypeIdx = 0; resourceTypeIdx < CardSet<ResourceCardType>.Values.Count; resourceTypeIdx++)
            {
                ResourceCardType resourceType = CardSet<ResourceCardType>.Values[resourceTypeIdx];
                uint bankStock = state.ResourceBank.Get(resourceType);

                uint totalAwardedAmount = 0;
                uint affectedPlayers = 0;

                for (int playerIdx = 0; playerIdx < state.Players.Length; playerIdx++)
                {
                    uint awardedAmount = yieldSummary[playerIdx, resourceTypeIdx];
                    totalAwardedAmount += awardedAmount;
                    affectedPlayers += awardedAmount > 0 ? 1u : 0u;
                }

                // Do not award yields, if bank stock is insufficient and more than one player is affected
                bool insufficientStock = bankStock < totalAwardedAmount;
                if (insufficientStock && affectedPlayers > 1)
                {
                    cappedYields += totalAwardedAmount;
                    continue;
                }

                // Transfer cards from bank to player
                for (int playerIdx = 0; playerIdx < state.Players.Length; playerIdx++)
                {
                    uint awardedAmount = yieldSummary[playerIdx, resourceTypeIdx];
                    if (awardedAmount > bankStock) awardedAmount = bankStock;

                    state.ResourceBank.Remove(resourceType, awardedAmount);
                    state.Players[playerIdx].ResourceCards.Add(resourceType, awardedAmount);
                }
            }

            return (yieldSummary, robbedYields, cappedYields);
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && turn.MustRoll;
        }


        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            RollAction randomRollAction = new RollAction(playerIdx, null);
            if(randomRollAction.IsValidFor(state)) actions.Add(randomRollAction);

            // TODO: Remove for exploration
            return actions;

            for(byte first = 1; first <= 6; first++)
            {
                for (byte second = 1; second <= 6; second++)
                {
                    RollResult roll = new() { First = first, Second = second };
                    RollAction action = new(state.Turn.PlayerIndex, roll);

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
            return base.ToString() + $", {RollResult.First}+{RollResult.Second}={RollResult.Total}";
        }
    }
}
