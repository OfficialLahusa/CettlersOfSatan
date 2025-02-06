using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class ThreeToOneTradeAction : Action, IActionProvider
    {
        public CardSet.CardType InputType { get; init; }
        public CardSet.CardType OutputType { get; init; }

        public ThreeToOneTradeAction(int playerIdx, CardSet.CardType inputType, CardSet.CardType outputType)
            : base(playerIdx)
        {
            InputType = inputType;
            OutputType = outputType;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove input from hand
            state.Players[PlayerIndex].CardSet.Remove(InputType, 3);

            // Remove output from bank
            state.Bank.Remove(OutputType, 1);

            // Add input to bank
            state.Bank.Add(InputType, 3);

            // Add output to hand
            state.Players[PlayerIndex].CardSet.Add(OutputType, 1);
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && !turn.MustMoveRobber;
        }

        public bool IsBoardValid(GameState state)
        {
            bool cardTypesDiffer = InputType != OutputType;
            bool usesResourceCards = CardSet.RESOURCE_CARD_TYPES.Contains(InputType) && CardSet.RESOURCE_CARD_TYPES.Contains(OutputType);
            bool playerHasInput = state.Players[PlayerIndex].CardSet.Contains(InputType, 3);
            bool bankHasOutput = state.Bank.Contains(OutputType, 1);

            // Check port privilege
            bool hasPort = state.Players[PlayerIndex].PortPrivileges.HasFlag(PortPrivileges.GenericThreeToOne);

            return cardTypesDiffer && usesResourceCards && playerHasInput && bankHasOutput && hasPort;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            foreach (var inputType in CardSet.RESOURCE_CARD_TYPES)
            {
                foreach (var outputType in CardSet.RESOURCE_CARD_TYPES)
                {
                    ThreeToOneTradeAction action = new(playerIdx, inputType, outputType);

                    if (action.IsBoardValid(state))
                    {
                        actions.Add(action);
                    }
                }
            }

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + $", {InputType.GetAbbreviation()}{InputType.GetAbbreviation()}{InputType.GetAbbreviation()} -> {OutputType.GetAbbreviation()}";
        }
    }
}
