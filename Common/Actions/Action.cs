using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public abstract class Action
    {
        // Index of the player performing the action
        public int PlayerIndex { get; init; }

        public Action(int playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public virtual void Apply(GameState state)
        {
            if (!IsValidFor(state)) throw new InvalidOperationException();
        }

        //public abstract void Revert(GameState state);

        public bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn) && IsBoardValid(state);
        }

        // Usually cheap to compute, can eliminate obviously illegal moves
        public abstract bool IsTurnValid(TurnState turn);

        // Often more expensive combinatorial checks, only check if turn state is valid
        public abstract bool IsBoardValid(GameState state);

        public override string ToString()
        {
            return $"{GetType().Name}: P{PlayerIndex}";
        }
    }
}
