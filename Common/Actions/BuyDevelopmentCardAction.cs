using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class BuyDevelopmentCardAction : Action, IActionProvider
    {
        public BuyDevelopmentCardAction(int playerIdx)
            : base(playerIdx)
        { }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove cards
            CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;
            playerCards.Remove(ResourceCardType.Wool, 1);
            playerCards.Remove(ResourceCardType.Grain, 1);
            playerCards.Remove(ResourceCardType.Ore, 1);

            // Return cards to bank
            state.ResourceBank.Add(ResourceCardType.Wool, 1);
            state.ResourceBank.Add(ResourceCardType.Grain, 1);
            state.ResourceBank.Add(ResourceCardType.Ore, 1);

            // Draw card from bank
            DevelopmentCardType drawnType = state.DevelopmentBank.Draw(true);

            // Add drawn card to player
            state.Players[PlayerIndex].DevelopmentCards.Add(drawnType, 1);

            // Add to list of new dev cards that cannot be played yet
            state.Players[PlayerIndex].NewDevelopmentCards.Add(drawnType, 1);

            // Award VP, if drawn
            if(drawnType == DevelopmentCardType.VictoryPoint)
            {
                state.Players[PlayerIndex].VictoryPoints.DevelopmentCardPoints++;

                // Check for match completion
                state.CheckForCompletion();
            }
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
            bool canAfford = state.Players[PlayerIndex].CanAffordDevelopmentCard();
            bool bankHasDevCards = state.DevelopmentBank.Count() > 0;

            return canAfford && bankHasDevCards;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            BuyDevelopmentCardAction action = new(playerIdx);
            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
