using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class MonopolyAction : Action, IActionProvider
    {
        ResourceCardType ChosenType { get; set; }

        public MonopolyAction(int playerIdx, ResourceCardType chosenType)
            : base(playerIdx)
        {
            ChosenType = chosenType;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove card
            state.Players[PlayerIndex].DevelopmentCards.Remove(DevelopmentCardType.Monopoly, 1);

            // Move cards of type to player
            for(int player = 0; player < state.Players.Length; player++)
            {
                if (player == PlayerIndex) continue;

                uint movedCount = state.Players[player].ResourceCards.Get(ChosenType);
                state.Players[player].ResourceCards.Remove(ChosenType, movedCount);
                state.Players[PlayerIndex].ResourceCards.Add(ChosenType, movedCount);
            }

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
            bool hasCard = state.Players[PlayerIndex].DevelopmentCards.Contains(DevelopmentCardType.Monopoly);

            // Check for dev card age
            bool cardAgeSufficient = state.Players[PlayerIndex].DevelopmentCards.Get(DevelopmentCardType.Monopoly) > state.Players[PlayerIndex].NewDevelopmentCards.Get(DevelopmentCardType.Monopoly);

            return hasCard && cardAgeSufficient;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = [];

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            foreach (var resourceType in CardSet<ResourceCardType>.Values)
            {
                MonopolyAction action = new(playerIdx, resourceType);

                if(action.IsBoardValid(state))
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
