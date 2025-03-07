﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class BuyDevelopmentCardAction : Action, IReplayAction, IActionProvider
    {
        public record BuyDevelopmentCardActionHistory(DevelopmentCardType DrawnType);

        public BuyDevelopmentCardActionHistory? History { get; private set; }

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
            DevelopmentCardType drawnType;

            // Randomly draw
            if (!HasHistory())
            {
                drawnType = state.DevelopmentBank.Draw(true);

                // Store random outcome
                History = new BuyDevelopmentCardActionHistory(drawnType);
            }
            // Replay stored draw
            else
            {
                drawnType = History!.DrawnType;

                state.DevelopmentBank.Remove(drawnType, 1);
            }

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

        public override void Revert(GameState state)
        {
            // Ensure action was applied before
            if (!HasHistory()) throw new InvalidOperationException();

            // Return cards
            CardSet<ResourceCardType> playerCards = state.Players[PlayerIndex].ResourceCards;
            playerCards.Add(ResourceCardType.Wool, 1);
            playerCards.Add(ResourceCardType.Grain, 1);
            playerCards.Add(ResourceCardType.Ore, 1);

            // Remove cards from bank
            state.ResourceBank.Remove(ResourceCardType.Wool, 1);
            state.ResourceBank.Remove(ResourceCardType.Grain, 1);
            state.ResourceBank.Remove(ResourceCardType.Ore, 1);

            DevelopmentCardType drawnType = History!.DrawnType;

            // Remove drawn card from player
            state.Players[PlayerIndex].DevelopmentCards.Remove(drawnType, 1);

            // Return drawn card to bank
            state.DevelopmentBank.Add(drawnType, 1);

            // Remove from list of new dev cards that cannot be played yet
            state.Players[PlayerIndex].NewDevelopmentCards.Remove(drawnType, 1);

            // Remove VP, if drawn
            if (drawnType == DevelopmentCardType.VictoryPoint)
            {
                state.Players[PlayerIndex].VictoryPoints.DevelopmentCardPoints--;

                // Reset to normal round
                state.Turn.TypeOfRound = TurnState.RoundType.Normal;
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

            // If action has history, ensure it is valid
            bool historyValid = !HasHistory() || state.DevelopmentBank.Get(History!.DrawnType) > 0;

            return canAfford && bankHasDevCards && historyValid;
        }

        public bool HasHistory()
        {
            return History != null;
        }

        public void ClearHistory()
        {
            History = null;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            BuyDevelopmentCardAction action = new(playerIdx);
            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
