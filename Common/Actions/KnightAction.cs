using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class KnightAction : Action, IActionProvider
    {
        public KnightAction(int playerIdx)
            : base(playerIdx)
        { }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove card
            state.Players[PlayerIndex].CardSet.Remove(CardSet.CardType.Knight, 1);

            // Update turn state
            state.Turn.MustMoveRobber = true;
            state.Turn.HasPlayedDevelopmentCard = true;

            // TODO: Increment largest army and recalculate VPs
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && !turn.MustMoveRobber
                && !turn.HasPlayedDevelopmentCard;
        }

        public override bool IsBoardValid(GameState state)
        {
            bool hasCard = state.Players[PlayerIndex].CardSet.Contains(CardSet.CardType.Knight);

            // TODO: Check for dev card age

            return hasCard;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            KnightAction action = new(state.Turn.PlayerIndex);

            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
