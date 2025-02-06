﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class YearOfPlentyAction : Action, IActionProvider
    {
        public CardSet.CardType FirstChoice { get; set; }
        public CardSet.CardType SecondChoice { get; set; }

        public YearOfPlentyAction(int playerIdx, CardSet.CardType firstChoice, CardSet.CardType secondChoice)
            : base(playerIdx)
        {
            FirstChoice = firstChoice;
            SecondChoice = secondChoice;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            CardSet playerCards = state.Players[PlayerIndex].CardSet;

            // Remove card
            playerCards.Remove(CardSet.CardType.YearOfPlenty, 1);

            // Move chosen cards from bank to hand
            state.Bank.Remove(FirstChoice, 1);
            state.Bank.Remove(SecondChoice, 1);
            playerCards.Add(FirstChoice, 1);
            playerCards.Add(SecondChoice, 1);

            // Update turn state
            state.Turn.HasPlayedDevelopmentCard = true;
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
                && !turn.MustMoveRobber
                && !turn.HasPlayedDevelopmentCard;
        }

        public bool IsBoardValid(GameState state)
        {
            bool hasCard = state.Players[PlayerIndex].CardSet.Contains(CardSet.CardType.YearOfPlenty);

            // Check for dev card age
            bool cardAgeSufficient = state.Players[PlayerIndex].CardSet.Get(CardSet.CardType.YearOfPlenty) > state.Players[PlayerIndex].NewDevelopmentCards[CardSet.CardType.YearOfPlenty - CardSet.CardType.Knight];

            bool validTypes = CardSet.RESOURCE_CARD_TYPES.Contains(FirstChoice) && CardSet.RESOURCE_CARD_TYPES.Contains(SecondChoice);

            bool bankHasCards = FirstChoice == SecondChoice ? state.Bank.Contains(FirstChoice, 2) : state.Bank.Contains(FirstChoice, 1) && state.Bank.Contains(SecondChoice, 1);

            return hasCard && cardAgeSufficient && validTypes && bankHasCards;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            if(!IsTurnValid(state.Turn, playerIdx)) return actions;

            foreach (var firstChoice in CardSet.RESOURCE_CARD_TYPES)
            {
                foreach (var secondChoice in CardSet.RESOURCE_CARD_TYPES)
                {
                    // Avoid adding the same pair twice
                    if (firstChoice > secondChoice) continue;

                    YearOfPlentyAction action = new(playerIdx, firstChoice, secondChoice);

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
            return base.ToString() + $", {FirstChoice.GetAbbreviation()}{SecondChoice.GetAbbreviation()}";
        }
    }
}
