
namespace Common
{
    public class GameSettings
    {
        // Maximum allowed number of resource cards before discarding is required
        public int RobberCardLimit { get; set; }
        // Required number of victory points to win the match
        public int VictoryPoints { get; set; }

        public GameSettings()
        {
            RobberCardLimit = 7;
            VictoryPoints = 10;
        }

        public override bool Equals(object? obj)
        {
            return obj is GameSettings settings &&
                   RobberCardLimit == settings.RobberCardLimit &&
                   VictoryPoints == settings.VictoryPoints;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RobberCardLimit, VictoryPoints);
        }
    }
}