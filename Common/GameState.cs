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
        public CardSet Bank { get; set; }
        public PlayerState[] Players { get; set; }
        public bool HasEnded => Turn.TypeOfRound == TurnState.RoundType.MatchEnded;

        public GameState(Board board, uint playerCount)
        {
            Settings = new GameSettings();
            Turn = new TurnState();
            Board = board;
            Bank = CardSet.CreateBank();
            Players = new PlayerState[playerCount];

            for(int i = 0; i < playerCount; i++)
            {
                Players[i] = new PlayerState();
            }
        }

        public (uint[,] yieldSummary, uint robbedYields, uint cappedYields) AwardYields(int number)
        {
            uint[,] yieldSummary = new uint[Players.Length, CardSet.RESOURCE_CARD_TYPES.Length];
            uint robbedYields = 0;

            // Calculate tile yields
            foreach (Tile tile in Board.Map.Where(x => x.HasYield() && x.Number == number))
            {
                foreach (Intersection intersection in tile.Intersections.Values)
                {
                    uint yieldCount = intersection.Building switch
                    {
                        Intersection.BuildingType.City => 2,
                        Intersection.BuildingType.Settlement => 1,
                        _ => 0
                    };

                    if (yieldCount > 0)
                    {
                        if (tile != Board.Robber)
                        {
                            yieldSummary[intersection.Owner, Array.IndexOf(CardSet.RESOURCE_CARD_TYPES, tile.Type.ToCardType())] += yieldCount;
                        }
                        else
                        {
                            robbedYields += yieldCount;
                        }
                    }
                }
            }

            // Award yields according to limited bank stock
            uint cappedYields = 0;
            for (int resourceTypeIdx = 0; resourceTypeIdx < CardSet.RESOURCE_CARD_TYPES.Length; resourceTypeIdx++)
            {
                CardSet.CardType resourceType = CardSet.RESOURCE_CARD_TYPES[resourceTypeIdx];
                uint bankStock = Bank.Get(resourceType);

                uint totalAwardedAmount = 0;
                uint affectedPlayers = 0;

                for (int playerIdx = 0; playerIdx < Players.Length; playerIdx++)
                {
                    uint awardedAmount = yieldSummary[playerIdx, resourceTypeIdx];
                    totalAwardedAmount += awardedAmount;
                    affectedPlayers += awardedAmount > 0 ? 1u : 0u;
                }

                // Do not award yields, if bank stock is insufficient and more than one player is affected
                bool insufficientStock = bankStock < totalAwardedAmount;
                if (insufficientStock && affectedPlayers > 1) 
                {
                    cappedYields += totalAwardedAmount;
                    continue;
                }

                // Transfer cards from bank to player
                for (int playerIdx = 0; playerIdx < Players.Length; playerIdx++)
                {
                    uint awardedAmount = yieldSummary[playerIdx, resourceTypeIdx];
                    if (awardedAmount > bankStock) awardedAmount = bankStock;

                    Bank.Remove(resourceType, awardedAmount);
                    Players[playerIdx].CardSet.Add(resourceType, awardedAmount);
                }
            }

            return (yieldSummary, robbedYields, cappedYields);
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
            return !Turn.MustDiscard && Turn.PlayerIndex == playerIdx || Turn.MustDiscard && Players[playerIdx].CardSet.GetResourceCardCount() > Settings.RobberCardLimit;
        }

        public void ResetCards()
        {
            Bank = CardSet.CreateBank();

            foreach(PlayerState player in Players)
            {
                player.CardSet.Clear();
            }
        }

        public void Reset()
        {
            Turn = new TurnState();
            Bank = CardSet.CreateBank();
            Players = new PlayerState[Players.Length];

            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new PlayerState();
            }
        }
    }
}
