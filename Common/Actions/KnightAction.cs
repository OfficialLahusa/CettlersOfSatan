﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class KnightAction : Action, IActionProvider
    {
        public KnightAction(int playerIdx)
            : base(playerIdx)
        { }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove card
            state.Players[PlayerIndex].CardSet.Remove(CardSet.CardType.Knight, 1);

            // Update turn state
            state.Turn.MustMoveRobber = true;
            state.Turn.HasPlayedDevelopmentCard = true;

            // Increment largest army and recalculate VPs
            ++state.Players[PlayerIndex].PlayedKnights;
            state.CalculateLargestArmy(PlayerIndex);

            // Check for match completion
            state.CheckForCompletion();
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
            bool hasCard = state.Players[PlayerIndex].CardSet.Contains(CardSet.CardType.Knight);

            // Check for dev card age
            bool cardAgeSufficient = state.Players[PlayerIndex].CardSet.Get(CardSet.CardType.Knight) > state.Players[PlayerIndex].NewDevelopmentCards[0];

            return hasCard && cardAgeSufficient;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            KnightAction action = new(playerIdx);

            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
