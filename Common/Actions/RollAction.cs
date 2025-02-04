using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class RollAction : Action, IActionProvider
    {
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

            // Robber triggered
            if (RollResult.Total == 7)
            {
                // Require discard if card limit is exceeded
                int requiredDiscards = 0;
                for (int player = 0; player < state.Players.Length; player++)
                {
                    if (state.Players[player].CardSet.GetResourceCardCount() > state.Settings.RobberCardLimit) requiredDiscards++;
                }
                state.Turn.AwaitedDiscards = requiredDiscards;

                // Require robber move
                state.Turn.MustMoveRobber = true;
            }
            // Normal yield
            else
            {
                state.AwardYields(RollResult.Total);
            }

            state.Turn.MustRoll = false;
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && turn.MustRoll;
        }

        public override bool IsBoardValid(GameState state)
        {
            // Does not depend on the board state
            return true;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            RollAction randomRollAction = new RollAction(state.Turn.PlayerIndex, null);
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
