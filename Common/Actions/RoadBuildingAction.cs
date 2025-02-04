using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Actions
{
    public class RoadBuildingAction : Action, IActionProvider
    {
        public RoadBuildingAction(int playerIdx)
            : base(playerIdx)
        { }

        public override void Apply(GameState state)
        {
            base.Apply(state);

            // Remove card
            state.Players[PlayerIndex].CardSet.Remove(CardSet.CardType.RoadBuilding, 1);

            // Add free road stock (up to 2)
            BuildingStock buildingStock = state.Players[PlayerIndex].BuildingStock;
            uint awardedAmount = Math.Min(buildingStock.RemainingRoads, 2);
            buildingStock.RemainingRoads -= awardedAmount;
            buildingStock.FreeRoads += awardedAmount;

            // Update turn state
            state.Turn.HasPlayedDevelopmentCard = true;

            // TODO: Recalculate longest road and award VPs
        }

        public override bool IsTurnValid(TurnState turn)
        {
            return turn.PlayerIndex == PlayerIndex
                && turn.TypeOfRound == TurnState.RoundType.Normal
                && !turn.MustRoll
                && !turn.MustDiscard
                && !turn.MustMoveRobber
                && !turn.HasPlayedDevelopmentCard;
        }

        public override bool IsBoardValid(GameState state)
        {
            bool hasCard = state.Players[PlayerIndex].CardSet.Contains(CardSet.CardType.RoadBuilding);

            // TODO: Check for dev card age

            return hasCard;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            RoadBuildingAction action = new(state.Turn.PlayerIndex);

            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
