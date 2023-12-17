using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Tile;

namespace Common
{
    public class MapGenerator
    {
        public static readonly Func<Tile, bool> LandTilePredicate;
        public static readonly Func<Tile, bool> YieldTilePredicate;
        public static readonly Func<Tile, bool> RedNumberPredicate;
        public static readonly Func<Tile, bool> BlackNumberPredicate;

        static MapGenerator()
        {
            LandTilePredicate       = tile => tile.IsLandTile();
            YieldTilePredicate      = tile => tile.HasYield();
            RedNumberPredicate      = tile => tile.Number.HasValue && (tile.Number.Value == 6 || tile.Number.Value == 8);
            BlackNumberPredicate    = tile => tile.Number.HasValue &&  tile.Number.Value != 6 && tile.Number.Value != 8;
        }

        public static HexMap<Tile> GenerateRandomClassic()
        {
            HexMap<Tile> map = new HexMap<Tile>(7, 7, new Tile(-1, -1, TileType.NonPlayable, null));

            // Shuffle tiles and number tokens
            List<TileType> types = new List<TileType>(){
                TileType.Brick, TileType.Brick, TileType.Brick,
                TileType.Lumber, TileType.Lumber, TileType.Lumber, TileType.Lumber,
                TileType.Ore, TileType.Ore, TileType.Ore,
                TileType.Grain, TileType.Grain, TileType.Grain, TileType.Grain,
                TileType.Wool, TileType.Wool, TileType.Wool, TileType.Wool,
                TileType.Desert
            };
            List<int> numbers = new List<int>()
            {
                2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12
            };
            Utils.Shuffle(types);
            Utils.Shuffle(numbers);

            // Assign tiles and tokens to map
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    // Distance from center (3, 3)
                    int dist = Coordinates.Distance(x, y, 3, 3);

                    // Land tiles
                    if (dist < 3)
                    {
                        TileType type = types[0];
                        types.RemoveAt(0);

                        int? number = null;
                        if (type != TileType.Desert)
                        {
                            number = numbers[0];
                            numbers.RemoveAt(0);
                        }

                        map.SetTile(x, y, new Tile(x, y, type, number));
                    }
                    // Water tiles
                    else if (dist < 4) map.SetTile(x, y, new Tile(x, y, TileType.Water, null));
                    // Non-playable tiles
                    else
                    {
                        map.SetTile(x, y, new Tile(x, y, TileType.NonPlayable, null));
                    }
                }
            }

            // Assign neighbors
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Tile tile = map.GetTile(x, y);
                    tile.Neighbors = map.GetNeighborsByDirection(x, y);
                }
            }

            // Prevent red number (6/8) adjacency
            HashSet<Tile> adjacentReds;
            while((adjacentReds = GetAdjacentRedNumbers(map)).Count > 0)
            {
                int redIdx = Utils.Random.Next(adjacentReds.Count);
                Tile red = adjacentReds.ElementAt(redIdx);

                HashSet<Tile> freeBlackNumbers = GetFreeBlackNumbers(map, red);

                int blackIdx = Utils.Random.Next(freeBlackNumbers.Count);
                Tile black = freeBlackNumbers.ElementAt(blackIdx);
                
                int? blackNum = black.Number;
                black.Number = red.Number;
                red.Number = blackNum;
            }

            return map;
        }

        private static HashSet<Tile> GetAdjacentRedNumbers(HexMap<Tile> map)
        {
            return map.Where(tile => RedNumberPredicate(tile) && map.GetNeighbors(tile.X, tile.Y, RedNumberPredicate).Count > 0);
        }

        private static HashSet<Tile> GetFreeBlackNumbers(HexMap<Tile> map, Tile redWorkingTile)
        {
            return map.Where(tile => {
                HashSet<Tile> redNeighbors = map.GetNeighbors(tile.X, tile.Y, RedNumberPredicate);

                // Remove the red tile to be swapped, because it is the only allowed red neighbor
                redNeighbors.Remove(redWorkingTile);

                return BlackNumberPredicate(tile) && redNeighbors.Count == 0;
            });
        }
    }
}
