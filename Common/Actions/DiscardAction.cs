using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class DiscardAction : Action
    {
        // Cards that are discarded by the player
        public CardSet SelectedCards;

        public DiscardAction(int playerIdx, CardSet selectedCards)
            : base(playerIdx)
        {
            SelectedCards = selectedCards;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove selected cards from hand
            CardSet cardSet = state.Players[PlayerIndex].CardSet;
            cardSet.Remove(SelectedCards);

            // Return discarded cards to bank
            state.Bank.Add(SelectedCards);

            // Decrement discard counter
            state.Turn.AwaitedDiscards--;
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            // Note: Player index does NOT need to match turn player index
            return turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && turn.MustDiscard;
        }

        public bool IsBoardValid(GameState state)
        {
            CardSet playerCards = state.Players[PlayerIndex].CardSet;

            int excessCards = (int)playerCards.GetResourceCardCount() - state.Settings.RobberCardLimit;

            // Terminate early if discarding isn't required
            if (excessCards <= 0) return false;

            bool validAmount = SelectedCards.GetResourceCardCount() == playerCards.GetResourceCardCount() / 2;
            bool validSubset = playerCards.Contains(SelectedCards);

            return validAmount && validSubset;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            if(!IsTurnValid(state.Turn, playerIdx)) return actions;

            CardSet playerCards = state.Players[playerIdx].CardSet;
            int excessCards = (int)playerCards.GetResourceCardCount() - state.Settings.RobberCardLimit;

            // Skip player, if no discard is needed
            if (excessCards <= 0) return actions;

            // Generate all discardable subsets
            ReadOnlySpan<CardSet.CardType> heldResources = CardSet.RESOURCE_CARD_TYPES
                .SelectMany(
                    resourceType => Enumerable.Repeat(resourceType, (int)playerCards.Get(resourceType))
                )
                .ToArray();

            List<CardSet.CardType[]> subsets = Utils.GetSubsets(heldResources, heldResources.Length / 2);

            // Create actions for each subset
            foreach (CardSet.CardType[] subset in subsets)
            {
                // Add to new CardSet
                CardSet cardSubset = new CardSet();
                foreach (var card in subset)
                {
                    cardSubset.Add(card, 1);
                }

                // Validate action
                DiscardAction action = new DiscardAction(playerIdx, cardSubset);
                if (action.IsBoardValid(state))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(", ");


            foreach(CardSet.CardType resourceType in CardSet.RESOURCE_CARD_TYPES)
            {
                for (int i = 0; i < SelectedCards.Get(resourceType); i++)
                {
                    sb.Append(resourceType.GetAbbreviation());
                }
            }

            return sb.ToString();
        }
    }
}
