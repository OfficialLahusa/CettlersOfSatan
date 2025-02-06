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

            // Allow newly acquired dev cards to be played
            Array.Fill<uint>(state.Players[PlayerIndex].NewDevelopmentCards, 0);

            // Update turn state
            state.Turn.PlayerIndex = ++state.Turn.PlayerIndex % state.Players.Length;
            if(state.Turn.PlayerIndex == 0) state.Turn.RoundCounter++;
            state.Turn.MustRoll = true;
            state.Turn.MustMoveRobber = false;
            state.Turn.AwaitedDiscards = 0;
            state.Turn.HasPlayedDevelopmentCard = false;

            // Players might have had enough VPs to win before their own turn
            state.CheckForCompletion();
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && !turn.MustMoveRobber;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            EndTurnAction action = new EndTurnAction(playerIdx);

            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
