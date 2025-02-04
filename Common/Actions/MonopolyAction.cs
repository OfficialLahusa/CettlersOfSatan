using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class MonopolyAction : Action, IActionProvider
    {
        CardSet.CardType ChosenType { get; set; }

        public MonopolyAction(int playerIdx, CardSet.CardType chosenType)
            : base(playerIdx)
        {
            ChosenType = chosenType;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove card
            state.Players[PlayerIndex].CardSet.Remove(CardSet.CardType.Monopoly, 1);

            // Move cards of type to player
            for(int player = 0; player < state.Players.Length; player++)
            {
                if (player == PlayerIndex) continue;

                uint movedCount = state.Players[player].CardSet.Get(ChosenType);
                state.Players[player].CardSet.Remove(ChosenType, movedCount);
                state.Players[PlayerIndex].CardSet.Add(ChosenType, movedCount);
            }

            // Update turn state
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
            bool hasCard = state.Players[PlayerIndex].CardSet.Contains(CardSet.CardType.Monopoly);

            bool validChoice = CardSet.RESOURCE_CARD_TYPES.Contains(ChosenType);

            // TODO: Check for dev card age

            return hasCard && validChoice;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [];

            foreach (var resourceType in CardSet.RESOURCE_CARD_TYPES)
            {
                MonopolyAction action = new(state.Turn.PlayerIndex, resourceType);

                if(action.IsValidFor(state))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + ", " + ChosenType.GetAbbreviation();
        }
    }
}
