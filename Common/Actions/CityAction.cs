﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class CityAction : Action, IActionProvider
    {
        public int IntersectionIndex { get; init; }

        public CityAction(int playerIdx, int intersectionIndex)
            : base(playerIdx)
        {
            IntersectionIndex = intersectionIndex;
        }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Place road
            Intersection intersection = state.Board.Intersections[IntersectionIndex];
            intersection.Owner = PlayerIndex;
            intersection.Building = Intersection.BuildingType.City;

            // Remove cards
            CardSet playerCards = state.Players[PlayerIndex].CardSet;
            playerCards.Remove(CardSet.CardType.Grain, 2);
            playerCards.Remove(CardSet.CardType.Ore, 3);

            // Return cards to bank
            state.Bank.Add(CardSet.CardType.Grain, 2);
            state.Bank.Add(CardSet.CardType.Ore, 3);

            // Remove piece from stock
            state.Players[PlayerIndex].BuildingStock.RemainingCities--;

            // Add previous piece to stock
            state.Players[PlayerIndex].BuildingStock.RemainingSettlements++;

            // Award VP
            state.Players[PlayerIndex].VictoryPoints.SettlementPoints--;
            state.Players[PlayerIndex].VictoryPoints.CityPoints += 2;

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
                && !turn.MustMoveRobber;
        }

        public bool IsBoardValid(GameState state)
        {
            Intersection intersection = state.Board.Intersections[IntersectionIndex];

            bool hasSettlement = intersection.Building == Intersection.BuildingType.Settlement;
            bool ownsSettlement = intersection.Owner == PlayerIndex;
            bool canAfford = state.Players[PlayerIndex].CanAffordCity();

            return hasSettlement && ownsSettlement && canAfford;
        }

        public static List<Action> GetActionsForState(GameState state, int playerIdx)
        {
            List<Action> actions = new List<Action>();

            if (!IsTurnValid(state.Turn, playerIdx)) return actions;

            for (int intersectionIdx = 0; intersectionIdx < state.Board.Intersections.Count; intersectionIdx++)
            {
                CityAction action = new(playerIdx, intersectionIdx);
                if (action.IsBoardValid(state))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        public override string ToString()
        {
            return base.ToString() + $", I{IntersectionIndex}";
        }
    }
}
