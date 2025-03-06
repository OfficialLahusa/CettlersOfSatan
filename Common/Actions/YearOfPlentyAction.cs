﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class YearOfPlentyAction : Action, IActionProvider
    {
        public ResourceCardType FirstChoice { get; set; }
        public ResourceCardType? SecondChoice { get; set; }

        public YearOfPlentyAction(int playerIdx, ResourceCardType firstChoice, ResourceCardType? secondChoice)
            : base(playerIdx)
        {
            FirstChoice = firstChoice;
            SecondChoice = secondChoice;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;

            // Remove card
            state.Players[PlayerIndex].DevelopmentCards.Remove(DevelopmentCardType.YearOfPlenty, 1);

            // Move chosen cards from bank to hand
            state.ResourceBank.Remove(FirstChoice, 1);
            playerCards.Add(FirstChoice, 1);

            if (SecondChoice.HasValue)
            {
                state.ResourceBank.Remove(SecondChoice.Value, 1);
                playerCards.Add(SecondChoice.Value, 1);
            }

            // Update turn state
            state.Turn.HasPlayedDevelopmentCard = true;
        }

        public override void Revert(GameState state)
        {
            CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;

            // Return card
            state.Players[PlayerIndex].DevelopmentCards.Add(DevelopmentCardType.YearOfPlenty, 1);

            // Return chosen cards from hand to bank
            state.ResourceBank.Add(FirstChoice, 1);
            playerCards.Remove(FirstChoice, 1);

            if (SecondChoice.HasValue)
            {
                state.ResourceBank.Add(SecondChoice.Value, 1);
                playerCards.Remove(SecondChoice.Value, 1);
            }

            // Update turn state
            state.Turn.HasPlayedDevelopmentCard = false;
        }

        public override bool IsValidFor(GameState state)
        {
            return IsTurnValid(state.Turn, PlayerIndex) && IsBoardValid(state);
        }

        public static bool IsTurnValid(TurnState turn, int playerIdx)
        {
            return turn.PlayerIndex == playerIdx
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustDiscard
                && !turn.MustMoveRobber
                && !turn.HasPlayedDevelopmentCard;
        }

        public bool IsBoardValid(GameState state)
        {
            bool hasCard = state.Players[PlayerIndex].DevelopmentCards.Contains(DevelopmentCardType.YearOfPlenty);

            // Check for dev card age
            bool cardAgeSufficient = state.Players[PlayerIndex].DevelopmentCards.Get(DevelopmentCardType.YearOfPlenty) > state.Players[PlayerIndex].NewDevelopmentCards.Get(DevelopmentCardType.YearOfPlenty);

            bool bankHasCards = FirstChoice == SecondChoice ? state.ResourceBank.Contains(FirstChoice, 2) : state.ResourceBank.Contains(FirstChoice, 1) && (!SecondChoice.HasValue || state.ResourceBank.Contains(SecondChoice.Value, 1));

            // Edge case: If bank only has a single resource left, second choice can be null
            bool secondChoiceValid = SecondChoice.HasValue || (!SecondChoice.HasValue && state.ResourceBank.Count() == 1);

            return hasCard && cardAgeSufficient && bankHasCards && secondChoiceValid;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            if(!IsTurnValid(state.Turn, playerIdx)) return actions;

            foreach (var firstChoice in CardSet<ResourceCardType>.Values)
            {
                foreach (var secondChoice in CardSet<ResourceCardType>.Values)
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

            // Edge case: If bank only has a single resource left, second choice can be null
            if (state.ResourceBank.Count() == 1)
            {
                foreach (var firstChoice in CardSet<ResourceCardType>.Values)
                {
                    if (state.ResourceBank.Get(firstChoice) == 1)
                    {
                        YearOfPlentyAction action = new(playerIdx, firstChoice, null);

                        if (action.IsBoardValid(state))
                        {
                            actions.Add(action);
                        }

                        break;
                    }
                }
            }

            return actions;
        }

        public override string ToString()
        {
            string secondChoiceAbbreviation = SecondChoice.HasValue ? SecondChoice.Value.GetAbbreviation() : "/";
            return base.ToString() + $", {FirstChoice.GetAbbreviation()}{secondChoiceAbbreviation}";
        }
    }
}
