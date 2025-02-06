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

        public abstract bool IsValidFor(GameState state);

        public override string ToString()
        {
            return $"{GetType().Name}: P{PlayerIndex}";
        }
    }
}
