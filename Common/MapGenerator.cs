using System.Net.Http.Headers;
using System.Numerics;
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

        public static Board GenerateRandomClassic(bool centerDesert = false)
        {
            HexMap<Tile> map = new HexMap<Tile>(7, 7, new Tile(-1, -1, TileType.NonPlayable, null));
            Tile? robber = null;

            // Shuffle tiles and number tokens
            List<TileType> tileTypes = new(){
                TileType.Brick, TileType.Brick, TileType.Brick,
                TileType.Lumber, TileType.Lumber, TileType.Lumber, TileType.Lumber,
                TileType.Ore, TileType.Ore, TileType.Ore,
                TileType.Grain, TileType.Grain, TileType.Grain, TileType.Grain,
                TileType.Wool, TileType.Wool, TileType.Wool, TileType.Wool
            };
            // Only shuffle desert if it isn't centered
            if (!centerDesert) tileTypes.Add(TileType.Desert);

            List<int> numberTokens = new()
            {
                2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12
            };

            List<Port.TradeType> portTypes = new()
            {
                Port.TradeType.Generic, Port.TradeType.Generic, Port.TradeType.Generic, Port.TradeType.Generic,
                Port.TradeType.Lumber, Port.TradeType.Brick, Port.TradeType.Wool, Port.TradeType.Grain, Port.TradeType.Ore
            };

            Utils.Shuffle(tileTypes);
            Utils.Shuffle(numberTokens);
            Utils.Shuffle(portTypes);

            // Assign tiles and tokens to map
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    // Distance from center (3, 3)
                    int dist = Coordinates.Distance(x, y, 3, 3);

                    // Center tile Desert option
                    if (centerDesert && dist == 0)
                    {
                        map.SetTile(x, y, new Tile(x, y, TileType.Desert, null));
                        robber = map.GetTile(x, y);
                    }
                    // Land tiles
                    else if (dist < 3)
                    {
                        TileType type = tileTypes[0];
                        tileTypes.RemoveAt(0);

                        int? number = null;
                        if (type != TileType.Desert)
                        {
                            number = numberTokens[0];
                            numberTokens.RemoveAt(0);
                        }

                        map.SetTile(x, y, new Tile(x, y, type, number));

                        if(type == TileType.Desert)
                        {
                            robber = map.GetTile(x, y);
                        }
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
                    tile.Neighbors = map.GetNeighborIndicesByDirection(x, y);
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

            // Assign intersections
            List<Intersection> intersections = new();

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Tile tile = map.GetTile(x, y);

                    // Only generate intersections on land tiles
                    if (!tile.IsLandTile()) continue;

                    foreach (Direction.Corner cornerDir in (Direction.Corner[])Enum.GetValues(typeof(Direction.Corner)))
                    {
                        (Direction.Tile left, Direction.Tile right) = cornerDir.GetAdjacentTiles();
                        (int lx, int ly) = Coordinates.Shift(x, y, left);
                        (int rx, int ry) = Coordinates.Shift(x, y, right);

                        Tile leftTile, rightTile;
                        int intersectionIdx = -1;

                        // Check if left tile is valid land
                        if (map.Contains(lx, ly) && (leftTile = map.GetTile(lx, ly)).IsLandTile())
                        {
                            // Check if neighbor already created the intersection
                            if (leftTile.Intersections.ContainsKey(cornerDir.Mirror().Rotate(-1)))
                            {
                                intersectionIdx = leftTile.Intersections[cornerDir.Mirror().Rotate(-1)];
                            }
                        }

                        // Check if left tile is valid land
                        if (intersectionIdx == -1 && map.Contains(rx, ry) && (rightTile = map.GetTile(rx, ry)).IsLandTile())
                        {
                            // Check if neighbor already created the intersection
                            if (rightTile.Intersections.ContainsKey(cornerDir.Mirror().Rotate(1)))
                            {
                                intersectionIdx = rightTile.Intersections[cornerDir.Mirror().Rotate(1)];
                            }
                        }

                        Intersection intersection;

                        // Create intersection if it doesn't already exist
                        if(intersectionIdx == -1)
                        {
                            intersection = new Intersection(cornerDir.HasDownwardsFacingIntersection());

                            // (Debug) Randomize building and owner
                            //intersection.Building = (Intersection.BuildingType)Math.Max(0, Utils.Random.Next(-2, 3));
                            //if (intersection.Building != Intersection.BuildingType.None) intersection.Owner = Utils.Random.Next(4);

                            intersections.Add(intersection);
                            intersectionIdx = intersections.Count - 1;
                        }
                        else
                        {
                            intersection = intersections[intersectionIdx];
                        }

                        // Register self at intersection
                        intersection.AdjacentTiles.Add(cornerDir, tile);

                        // Register intersection at self
                        tile.Intersections.Add(cornerDir, intersectionIdx);
                    }
                }
            }

            // Assign edges
            List<Edge> edges = new();

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Tile tile = map.GetTile(x, y);

                    // Only generate edges that originate from a land tile
                    if (!tile.IsLandTile()) continue;

                    foreach (Direction.Tile tileDir in (Direction.Tile[])Enum.GetValues(typeof(Direction.Tile)))
                    {
                        (int px, int py) = Coordinates.Shift(x, y, tileDir);

                        // Adjacent tile on the other side of the handled edge
                        Tile adjacentTile;
                        int edgeIdx = -1;

                        if(map.Contains(px, py))
                        {
                            adjacentTile = map.GetTile(px, py);
                            // Check if neighbor already created the handled edge
                            if (adjacentTile.Edges.ContainsKey(tileDir.Mirror()))
                            {
                                edgeIdx = adjacentTile.Edges[tileDir.Mirror()];
                            }
                        }

                        // Create edge if doesn't already exist
                        Edge edge;
                        if (edgeIdx == -1)
                        {
                            edge = new Edge(tileDir.ToEdgeDir());

                            // (Debug) Randomize building and owner
                            //edge.Building = (Edge.BuildingType)Math.Max(0, Utils.Random.Next(-3, 2));
                            //if (edge.Building != Edge.BuildingType.None) edge.Owner = Utils.Random.Next(4);

                            edges.Add(edge);
                            edgeIdx = edges.Count - 1;
                        }
                        else
                        {
                            edge = edges[edgeIdx];
                        }

                        // Register self at edge
                        if (tileDir.IsEast())
                        {
                            edge.WestTile = tile;
                        }
                        else
                        {
                            edge.EastTile = tile;
                        }

                        // Register edge at self
                        tile.Edges.Add(tileDir, edgeIdx);
                    }
                }
            }

            // Place ports in fixed locations with random types
            List<Port> ports = new();
            // Hard-coded positions and orientations of ports
            List<(int q, int r, int s, Direction.Tile dir)> portPositions = new()
            {
                (-2, -1,  3, Direction.Tile.East),
                ( 0, -3,  3, Direction.Tile.SouthEast),
                ( 2, -3,  1, Direction.Tile.SouthWest),
                ( 3, -2, -1, Direction.Tile.SouthWest),
                ( 3,  0, -3, Direction.Tile.West),
                ( 1,  2, -3, Direction.Tile.NorthWest),
                (-1,  3, -2, Direction.Tile.NorthWest),
                (-3,  3,  0, Direction.Tile.NorthEast),
                (-3,  1,  2, Direction.Tile.East)
            };
            foreach(var portPlacement in portPositions)
            {
                // Calculate port coordinates
                Vector3 centerPos = Coordinates.EvenRToCube(3, 3);
                Vector3 portPos = new Vector3(centerPos.X + portPlacement.q, centerPos.Y + portPlacement.r, centerPos.Z + portPlacement.s);
                (int x, int y) = Coordinates.CubeToEvenR(portPos);
                Tile portTile = map.GetTile(x, y);

                // Initialize port with random type from shuffled list
                Port port = new Port(portTile, portPlacement.dir, portTypes[0]);
                ports.Add(port);
                portTypes.RemoveAt(0);
            }

            return new Board(map, intersections, edges, ports, robber);
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
