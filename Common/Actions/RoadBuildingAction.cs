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

            // Check for dev card age
            bool cardAgeSufficient = state.Players[PlayerIndex].CardSet.Get(CardSet.CardType.RoadBuilding) > state.Players[PlayerIndex].NewDevelopmentCards[CardSet.CardType.RoadBuilding - CardSet.CardType.Knight];

            return hasCard && cardAgeSufficient;
        }

        public static List<Action> GetActionsForState(GameState state)
        {
            RoadBuildingAction action = new(state.Turn.PlayerIndex);

            return action.IsValidFor(state) ? [action] : [];
        }
    }
}
