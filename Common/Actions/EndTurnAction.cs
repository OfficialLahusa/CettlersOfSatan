using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class EndTurnAction : Action, IActionProvider
    {
        public record EndTurnActionOutcome(uint PrevFreeRoads, CardSet<DevelopmentCardType> PrevNewDevelopmentCards, bool PrevHasPlayedDevelopmentCard);

        public EndTurnActionOutcome? Outcome { get; private set; }

        public EndTurnAction(int playerIdx)
            : base(playerIdx)
        { }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Ensure action was not applied before
            if (Outcome != null) throw new InvalidOperationException();

            Outcome = new EndTurnActionOutcome(
                state.Players[PlayerIndex].BuildingStock.FreeRoads,
                state.Players[PlayerIndex].NewDevelopmentCards,
                state.Turn.HasPlayedDevelopmentCard
            );

            // Return remaining free stock from road building dev card to main stock
            BuildingStock stock = state.Players[PlayerIndex].BuildingStock;
            if(stock.FreeRoads > 0)
            {
                stock.RemainingRoads += stock.FreeRoads;
                stock.FreeRoads = 0;
            }

            // Allow newly acquired dev cards to be played
            state.Players[PlayerIndex].NewDevelopmentCards = new();

            // Update turn state
            state.Turn.PlayerIndex = ++state.Turn.PlayerIndex % state.Players.Length;
            if(state.Turn.PlayerIndex == 0) state.Turn.RoundCounter++;
            state.Turn.MustRoll = true;
            // TODO: Remove?
            state.Turn.MustMoveRobber = false;
            Array.Fill(state.Turn.AwaitedPlayerDiscards, false);
            state.Turn.HasPlayedDevelopmentCard = false;

            // Players might have had enough VPs to win before their own turn
            state.CheckForCompletion();
        }

        public override void Revert(GameState state)
        {
            // Ensure action was applied before
            if (Outcome == null) throw new InvalidOperationException();

            // Return previous free roadbuilding stock
            BuildingStock stock = state.Players[PlayerIndex].BuildingStock;
            if (Outcome.PrevFreeRoads > 0)
            {
                stock.RemainingRoads -= Outcome.PrevFreeRoads;
                stock.FreeRoads = Outcome.PrevFreeRoads;
            }

            // Block previous new dev cards
            state.Players[PlayerIndex].NewDevelopmentCards = Outcome.PrevNewDevelopmentCards;

            // Update turn state
            if (state.Turn.PlayerIndex == 0) state.Turn.RoundCounter--;
            state.Turn.PlayerIndex--;
            if (state.Turn.PlayerIndex < 0) state.Turn.PlayerIndex = state.Players.Length - 1;
            state.Turn.MustRoll = false;
            state.Turn.HasPlayedDevelopmentCard = Outcome.PrevHasPlayedDevelopmentCard;

            // Un-complete match
            state.Turn.TypeOfRound = TurnState.RoundType.Normal;
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
