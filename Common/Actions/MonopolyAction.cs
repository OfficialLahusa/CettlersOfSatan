﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class MonopolyAction : Action, IActionProvider
    {
        public record MonopolyActionOutcome(ReadOnlyCollection<(int, uint)> TransferredCards);

        public MonopolyActionOutcome? Outcome { get; private set; }

        public ResourceCardType ChosenType { get; set; }

        public MonopolyAction(int playerIdx, ResourceCardType chosenType)
            : base(playerIdx)
        {
            ChosenType = chosenType;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Ensure action was not applied before
            if (Outcome != null) throw new InvalidOperationException();

            // Remove card
            state.Players[PlayerIndex].DevelopmentCards.Remove(DevelopmentCardType.Monopoly, 1);

            // Move cards of type to player
            List<(int, uint)> transferredCards = [];
            for(int otherPlayerIdx = 0; otherPlayerIdx < state.Players.Length; otherPlayerIdx++)
            {
                if (otherPlayerIdx == PlayerIndex) continue;

                uint movedCount = state.Players[otherPlayerIdx].ResourceCards.Get(ChosenType);

                if (movedCount == 0) continue;

                state.Players[otherPlayerIdx].ResourceCards.Remove(ChosenType, movedCount);
                state.Players[PlayerIndex].ResourceCards.Add(ChosenType, movedCount);

                transferredCards.Add((otherPlayerIdx, movedCount));
            }

            Outcome = new MonopolyActionOutcome(transferredCards.OrderByDescending(x => (x.Item2, -x.Item1)).ToList().AsReadOnly());

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
