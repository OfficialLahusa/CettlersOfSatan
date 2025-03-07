using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Direction;

namespace Common
{
    public class GameState
    {
        public GameSettings Settings { get; set; }
        public TurnState Turn { get; set; }
        public Board Board { get; set; }
        public CardSet<ResourceCardType> ResourceBank { get; set; }
        public CardSet<DevelopmentCardType> DevelopmentBank { get; set; }
        public PlayerState[] Players { get; set; }
        public bool HasEnded => Turn.TypeOfRound == TurnState.RoundType.MatchEnded;

        public GameState(Board board, uint playerCount)
        {
            Settings = new GameSettings();
            Turn = new TurnState(playerCount);
            Board = board;
            (ResourceBank, DevelopmentBank) = CreateBank();
            Players = new PlayerState[playerCount];

            for(int i = 0; i < playerCount; i++)
            {
                Players[i] = new PlayerState();
            }
        }

        public void CalculateLargestArmy(int causingPlayerIdx)
        {
            uint playerKnights = Players[causingPlayerIdx].PlayedKnights;

            // Abort if minimum army size wasn't reached
            if (playerKnights < 3) return;

            // Minimum reached => Abort if player isn't ahead in army size
            for (int playerIdx = 0; playerIdx < Players.Length; playerIdx++)
            {
                if (playerIdx != causingPlayerIdx && Players[playerIdx].PlayedKnights >= playerKnights)
                {
                    return;
                }
            }
            
            // Army is largest => Reallocate points to player
            for (int playerIdx = 0; playerIdx < Players.Length; playerIdx++)
            {
                Players[playerIdx].VictoryPoints.LargestArmyPoints = (byte)(playerIdx == causingPlayerIdx ? 2 : 0);
            }
        }

        public void CalculateLongestRoad(int causingPlayerIdx, bool checkForBreak = false)
        {
            Dictionary<Edge, int> roadIndexLookup = Board.Edges
                .Select((edge, idx) => (edge, idx))
                .Where(x => x.edge.Owner == causingPlayerIdx && x.edge.Building != Edge.BuildingType.None)
                .ToDictionary(x => x.edge, x => x.idx);

            ImmutableHashSet<Edge> playerRoads = roadIndexLookup.Keys.ToImmutableHashSet();
            HashSet<Edge> longestPlayerRoad = [];

            // Recursively calculate longest road from each possible starting road
            foreach (Edge startingRoad in playerRoads)
            {
                HashSet<Edge> candidate = CalculateLongestRoadRec(causingPlayerIdx, startingRoad, playerRoads.Remove(startingRoad), []);
                if (candidate.Count > longestPlayerRoad.Count)
                {
                    longestPlayerRoad = candidate;

                    // Skip remaining branches, if the candidate length is guaranteed to be maximal
                    // => No longer road achievable, only permutations
                    if (longestPlayerRoad.Count == playerRoads.Count) break;
                }
            }

            uint roadLength = (uint)longestPlayerRoad.Count;
            Players[causingPlayerIdx].LongestRoadLength = roadLength;

            // Award VPs
            // Cause: Road broken by settlement placement
            if (checkForBreak)
            {
                // Check if player was the leader
                if (Players[causingPlayerIdx].VictoryPoints.LongestRoadPoints == 0) return;

                // Check for minimum length
                uint globalLongestRoad = Players.Max(state => state.LongestRoadLength);
                if (globalLongestRoad < 5) return;

                bool isLeading = roadLength == globalLongestRoad;
                bool isTied = Players.Count(state => state.LongestRoadLength == globalLongestRoad) > 1;

                // Keep VPs if still higher or tied
                if (isLeading) return;

                // Set VPs aside if tied and behind the tie
                if (!isLeading && isTied)
                {
                    Players[causingPlayerIdx].VictoryPoints.LongestRoadPoints = 0;
                    return;
                }

                // Give VPs to new leader if behind and untied
                for (int playerIdx = 0; playerIdx < Players.Length; playerIdx++)
                {
                    bool isNewLeader = Players[playerIdx].LongestRoadLength == globalLongestRoad;
                    Players[playerIdx].VictoryPoints.LongestRoadPoints = (byte)(isNewLeader ? 2 : 0);
                }
            }
            // Cause: New road placed
            else
            {
                // Check for minimum length
                if (roadLength < 5) return;

                // Check if another player has at least an equally long road
                if (Players.Any(player => player != Players[causingPlayerIdx] && player.LongestRoadLength >= roadLength)) return;

                // Move VPs to player
                for (int playerIdx = 0; playerIdx < Players.Length; playerIdx++)
                {
                    Players[playerIdx].VictoryPoints.LongestRoadPoints = (byte)(playerIdx == causingPlayerIdx ? 2 : 0);
                }
            }
        }

        private static HashSet<Edge> CalculateLongestRoadRec(int playerIdx, Edge current, ImmutableHashSet<Edge> remaining, ImmutableHashSet<Edge> contained)
        {
            HashSet<Edge> longestPlayerRoad = [.. contained, current];

            // Terminate if all player roads are contained
            if (remaining.IsEmpty) return longestPlayerRoad;

            // Find possible branches
            (Intersection top, Intersection bottom) = current.Intersections;
            bool topBlocked = top.Owner != playerIdx && top.Building != Intersection.BuildingType.None;
            bool bottomBlocked = bottom.Owner != playerIdx && bottom.Building != Intersection.BuildingType.None;

            var topRoads = top.AdjacentEdges.Values.Where(edge => edge.Owner == playerIdx && edge != current);
            var bottomRoads = bottom.AdjacentEdges.Values.Where(edge => edge.Owner == playerIdx && edge != current);

            bool topAlreadyConnected = topRoads.Any(contained.Contains);
            bool bottomAlreadyConnected = bottomRoads.Any(contained.Contains);

            var remainingTopRoads = topRoads.Intersect(remaining);
            var remainingBottomRoads = bottomRoads.Intersect(remaining);

            HashSet<Edge> possibleBranches = [];
            if (!topBlocked && !topAlreadyConnected) possibleBranches.UnionWith(remainingTopRoads);
            if (!bottomBlocked && !bottomAlreadyConnected) possibleBranches.UnionWith(remainingBottomRoads);

            // Recursively evaluate branches
            foreach (Edge branch in possibleBranches)
            {
                HashSet<Edge> candidate = CalculateLongestRoadRec(playerIdx, branch, remaining.Remove(current), [.. contained, current]);
                if (candidate.Count > longestPlayerRoad.Count)
                {
                    longestPlayerRoad = candidate;

                    // Skip remaining branches, if the candidate length is guaranteed to be maximal
                    // => No longer road achievable, only permutations
                    if (longestPlayerRoad.Count == contained.Count + remaining.Count) break;
                }
            }

            return longestPlayerRoad;
        }

        public void CheckForCompletion()
        {
            // Players can only win on their own turn
            if (Players[Turn.PlayerIndex].VictoryPoints.Total >= Settings.VictoryPoints)
            {
                Turn.TypeOfRound = TurnState.RoundType.MatchEnded;
            }
        }

        public bool CanPlayerAct(int playerIdx)
        {
            // TODO: Eventually account for trade offers from other players to target player
            return !Turn.MustDiscard && Turn.PlayerIndex == playerIdx || Turn.MustDiscard && Turn.AwaitedPlayerDiscards[playerIdx];
        }

        public static (CardSet<ResourceCardType> resources, CardSet<DevelopmentCardType> development) CreateBank()
        {
            CardSet<ResourceCardType> resources = new();
            CardSet<DevelopmentCardType> development = new();

            resources.Add(ResourceCardType.Lumber, 19);
            resources.Add(ResourceCardType.Brick, 19);
            resources.Add(ResourceCardType.Wool, 19);
            resources.Add(ResourceCardType.Grain, 19);
            resources.Add(ResourceCardType.Ore, 19);

            development.Add(DevelopmentCardType.Knight, 14);
            development.Add(DevelopmentCardType.RoadBuilding, 2);
            development.Add(DevelopmentCardType.YearOfPlenty, 2);
            development.Add(DevelopmentCardType.Monopoly, 2);
            development.Add(DevelopmentCardType.VictoryPoint, 5);

            return (resources, development);
        }

        public void ResetCards()
        {
            (ResourceBank, DevelopmentBank) = CreateBank();

            foreach(PlayerState player in Players)
            {
                player.ResourceCards = new();
                player.DevelopmentCards = new();
            }
        }

        public void Reset()
        {
            Turn = new TurnState((uint)Players.Length);
            (ResourceBank, DevelopmentBank) = CreateBank();
            Players = new PlayerState[Players.Length];

            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new PlayerState();
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is GameState other
                && Settings.Equals(other.Settings)
                && Turn.Equals(other.Turn)
                && Board.Equals(other.Board)
                && ResourceBank.Equals(other.ResourceBank)
                && DevelopmentBank.Equals(other.DevelopmentBank)
                && Players.SequenceEqual(other.Players);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            foreach (PlayerState player in Players)
            {
                hash.Add(player);
            }

            hash.Add(Settings);
            hash.Add(Turn);
            hash.Add(Board);
            hash.Add(ResourceBank);
            hash.Add(DevelopmentBank);

            return hash.ToHashCode();
        }
    }
}
