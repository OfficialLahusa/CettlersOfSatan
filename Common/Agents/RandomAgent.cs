using Common.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Common.Actions.Action;

namespace Common.Agents
{
    public class RandomAgent : Agent
    {
        public RandomAgent(int playerIndex)
            : base(playerIndex)
        {

        }

        public override Action Act(GameState state)
        {
            List<Action> actions = [
                .. EndTurnAction.GetActionsForState(state, PlayerIndex),

                .. RollAction.GetActionsForState(state, PlayerIndex),
                .. RobberAction.GetActionsForState(state, PlayerIndex),

                .. FirstInitialSettlementAction.GetActionsForState(state, PlayerIndex),
                .. FirstInitialRoadAction.GetActionsForState(state, PlayerIndex),
                .. SecondInitialSettlementAction.GetActionsForState(state, PlayerIndex),
                .. SecondInitialRoadAction.GetActionsForState(state, PlayerIndex),

                .. RoadAction.GetActionsForState(state, PlayerIndex),
                .. SettlementAction.GetActionsForState(state, PlayerIndex),
                .. CityAction.GetActionsForState(state, PlayerIndex),
                .. BuyDevelopmentCardAction.GetActionsForState(state, PlayerIndex),

                .. KnightAction.GetActionsForState(state, PlayerIndex),
                .. MonopolyAction.GetActionsForState(state, PlayerIndex),
                .. RoadBuildingAction.GetActionsForState(state, PlayerIndex),
                .. YearOfPlentyAction.GetActionsForState(state, PlayerIndex),

                .. FourToOneTradeAction.GetActionsForState(state, PlayerIndex),
                .. ThreeToOneTradeAction.GetActionsForState(state, PlayerIndex),
                .. TwoToOneTradeAction.GetActionsForState(state, PlayerIndex)
            ];

            if(DiscardAction.IsTurnValid(state.Turn, PlayerIndex))
            {
                actions.Add(GetRandomDiscard(state));
            }

            if (actions.Count == 0) throw new InvalidOperationException();

            int minIdx = actions.Count > 1 && actions[0] is EndTurnAction ? 1 : 0;
            int actionIdx = Utils.Random.Next(minIdx, actions.Count);
            return actions[actionIdx];
        }

        private DiscardAction GetRandomDiscard(GameState state)
        {
            // Get list of types of held resource cards
            List<CardSet.CardType> heldResources = CardSet.RESOURCE_CARD_TYPES
                .SelectMany(
                    resourceType => Enumerable.Repeat(resourceType, (int)state.Players[PlayerIndex].CardSet.Get(resourceType))
                )
                .ToList();
            
            // Randomize card order
            Utils.Shuffle(heldResources);

            // Remove the cards that are kept
            heldResources.RemoveRange(0, heldResources.Count - heldResources.Count / 2);

            // Add to new CardSet
            CardSet discardedSet = new CardSet();
            foreach (var card in heldResources)
            {
                discardedSet.Add(card, 1);
            }

            return new DiscardAction(PlayerIndex, discardedSet);
        }
    }
}
