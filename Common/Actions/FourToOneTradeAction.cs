using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class FourToOneTradeAction : Action, IActionProvider
    {
        public CardSet.CardType InputType { get; init; }
        public CardSet.CardType OutputType { get; init; }

        public FourToOneTradeAction(int playerIdx, CardSet.CardType inputType, CardSet.CardType outputType)
            : base(playerIdx)
        {
            InputType = inputType;
            OutputType = outputType;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove input from hand
            state.Players[PlayerIndex].CardSet.Remove(InputType, 4);

            // Remove output from bank
            state.Bank.Remove(OutputType, 1);

            // Add input to bank
            state.Bank.Add(InputType, 4);

            // Add output to hand
            state.Players[PlayerIndex].CardSet.Add(OutputType, 1);
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
            bool cardTypesDiffer = InputType != OutputType;
            bool usesResourceCards = CardSet.RESOURCE_CARD_TYPES.Contains(InputType) && CardSet.RESOURCE_CARD_TYPES.Contains(OutputType);
            bool playerHasInput = state.Players[PlayerIndex].CardSet.Contains(InputType, 4);
            bool bankHasOutput = state.Bank.Contains(OutputType, 1);

            return cardTypesDiffer && usesResourceCards && playerHasInput && bankHasOutput;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            foreach (var inputType in CardSet.RESOURCE_CARD_TYPES)
            {
                foreach (var outputType in CardSet.RESOURCE_CARD_TYPES)
                {
                    FourToOneTradeAction action = new(state.Turn.PlayerIndex, inputType, outputType);

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
            return base.ToString() + $", {InputType.GetAbbreviation()}{InputType.GetAbbreviation()}{InputType.GetAbbreviation()}{InputType.GetAbbreviation()} -> {OutputType.GetAbbreviation()}";
        }
    }
}
