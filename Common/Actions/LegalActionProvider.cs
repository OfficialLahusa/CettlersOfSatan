using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class LegalActionProvider : IActionProvider
    {
        public static List<Action> GetActionsForState(GameState state)
        {
            List<Action> actions = [
                .. EndTurnAction.GetActionsForState(state),

                .. RollAction.GetActionsForState(state),
                .. DiscardAction.GetActionsForState(state),
                .. RobberAction.GetActionsForState(state),

                .. RoadAction.GetActionsForState(state),
                .. SettlementAction.GetActionsForState(state),
                .. CityAction.GetActionsForState(state),
                .. BuyDevelopmentCardAction.GetActionsForState(state),

                .. KnightAction.GetActionsForState(state),
                .. MonopolyAction.GetActionsForState(state),
                .. RoadBuildingAction.GetActionsForState(state),
                .. YearOfPlentyAction.GetActionsForState(state),

                .. FourToOneTradeAction.GetActionsForState(state),
                .. ThreeToOneTradeAction.GetActionsForState(state),
                .. TwoToOneTradeAction.GetActionsForState(state)
            ];

            return actions;
        }
    }
}
