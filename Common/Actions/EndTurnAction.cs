using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class EndTurnAction : Action, IActionProvider
    {
        public EndTurnAction(int playerIdx)
            : base(playerIdx)
        { }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Return remaining free stock from road building dev card to main stock
            BuildingStock stock = state.Players[PlayerIndex].BuildingStock;
            if(stock.FreeRoads > 0)
            {
                stock.RemainingRoads += stock.FreeRoads;
                stock.FreeRoads = 0;
            }

            // Update turn state
            int nextPlayerIdx = ++state.Turn.PlayerIndex % state.Players.Length;
            state.Turn = new TurnState(nextPlayerIdx, ++state.Turn.RoundCounter);
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
            // Does not depend on the board state
            return true;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            EndTurnAction action = new EndTurnAction(state.Turn.PlayerIndex);

            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
