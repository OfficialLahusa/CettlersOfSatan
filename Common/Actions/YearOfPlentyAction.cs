using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class YearOfPlentyAction : Action, IActionProvider
    {
        public ResourceCardType FirstChoice { get; set; }
        public ResourceCardType SecondChoice { get; set; }

        public YearOfPlentyAction(int playerIdx, ResourceCardType firstChoice, ResourceCardType secondChoice)
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
            state.ResourceBank.Remove(SecondChoice, 1);
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
                && !turn.MustDiscard
                && !turn.MustMoveRobber
                && !turn.HasPlayedDevelopmentCard;
        }

        public bool IsBoardValid(GameState state)
        {
            bool hasCard = state.Players[PlayerIndex].DevelopmentCards.Contains(DevelopmentCardType.YearOfPlenty);

            // Check for dev card age
            bool cardAgeSufficient = state.Players[PlayerIndex].DevelopmentCards.Get(DevelopmentCardType.YearOfPlenty) > state.Players[PlayerIndex].NewDevelopmentCards.Get(DevelopmentCardType.YearOfPlenty);

            bool bankHasCards = FirstChoice == SecondChoice ? state.ResourceBank.Contains(FirstChoice, 2) : state.ResourceBank.Contains(FirstChoice, 1) && state.ResourceBank.Contains(SecondChoice, 1);

            return hasCard && cardAgeSufficient && bankHasCards;
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

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + $", {FirstChoice.GetAbbreviation()}{SecondChoice.GetAbbreviation()}";
        }
    }
}
