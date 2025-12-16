using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Common
{
    using AdjIdx = byte;

    public class AdjacencyMatrix
    {
        protected const AdjIdx NO_ADJACENCY = 255;
        protected static readonly ArrayEqualityComparer<AdjIdx> _arrayEqualityComparer;

        // -- References to board elements --
        // Not serialized, restored via Attach(Board) after deserialization
        public HexMap<Tile> Map;
        public List<Intersection> Intersections;
        public List<Edge> Edges;

        // Dim 1: Source tile
        // Dim 2: Tile direction
        // Stored: Flattened index of adjacent tile (y * width + x)
        protected AdjIdx[][] _tileToTile;

        // Dim 1: Source tile
        // Dim 2: Corner direction
        // Stored: Index of adjacent intersection
        protected AdjIdx[][] _tileToIntersection;

        // Dim 1: Source tile
        // Dim 2: Tile direction
        // Stored: Index of adjacent edge
        protected AdjIdx[][] _tileToEdge;

        // Dim 1: Source intersection
        // Dim 2: Corner direction (that the source intersection is on at the adjacent tile)
        // Stored: Tile index of adjacent tile
        protected List<AdjIdx[]?> _intersectionToTile;

        // Dim 1: Source edge
        // Stored: Tile index of adjacent tile
        protected List<AdjIdx> _edgeToWestTile;

        // Dim 1: Source edge
        // Stored: Tile index of adjacent tile
        protected List<AdjIdx> _edgeToEastTile;

        // -- Cache --
        // Dim 1: Source edge
        // Stored: (top intersection, bottom intersection)
        protected List<(Intersection top, Intersection bottom)?> _edgeToIntersection;

        // -- Cache --
        // Dim 1: Source intersection
        // Stored: Set of adjacent edges
        protected List<HashSet<Edge>?> _intersectionToEdge;

        static AdjacencyMatrix()
        {
            _arrayEqualityComparer = new ArrayEqualityComparer<AdjIdx>();
        }

        public AdjacencyMatrix(HexMap<Tile> map, List<Intersection> intersections, List<Edge> edges)
        {
            Map = map;
            Intersections = intersections;
            Edges = edges;

            _tileToTile = new AdjIdx[Map.Width * Map.Height][];
            _tileToIntersection = new AdjIdx[Map.Width * Map.Height][];
            _tileToEdge = new AdjIdx[Map.Width * Map.Height][];
            _intersectionToTile = new List<AdjIdx[]?>();
            _edgeToWestTile = new List<AdjIdx>();
            _edgeToEastTile = new List<AdjIdx>();

            // -- Cache --
            _edgeToIntersection = new List<(Intersection top, Intersection bottom)?>();
            _intersectionToEdge = new List<HashSet<Edge>?>();
        }

        /// <summary>
        /// Deep copy constructor.
        /// Assumes map, intersections and edges to be cloned beforehand and does not clone them here.
        /// </summary>
        /// <param name="copy">Instance to copy</param>
        public AdjacencyMatrix(AdjacencyMatrix copy, HexMap<Tile> map, List<Intersection> intersections, List<Edge> edges)
            : this (map, intersections, edges)
        {
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                if (copy._tileToTile[i] != null)
                {
                    _tileToTile[i] = new AdjIdx[copy._tileToTile[i].Length];
                    Array.Copy(copy._tileToTile[i], _tileToTile[i], copy._tileToTile[i].Length);
                }
                if (copy._tileToIntersection[i] != null)
                {
                    _tileToIntersection[i] = new AdjIdx[copy._tileToIntersection[i].Length];
                    Array.Copy(copy._tileToIntersection[i], _tileToIntersection[i], copy._tileToIntersection[i].Length);
                }
                if (copy._tileToEdge[i] != null)
                {
                    _tileToEdge[i] = new AdjIdx[copy._tileToEdge[i].Length];
                    Array.Copy(copy._tileToEdge[i], _tileToEdge[i], copy._tileToEdge[i].Length);
                }
            }

            for (int i = 0; i < copy._intersectionToTile.Count; i++)
            {
                if (copy._intersectionToTile[i] == null)
                    _intersectionToTile.Add(null);
                else
                {
                    AdjIdx[] entry = new AdjIdx[copy._intersectionToTile[i]!.Length];
                    Array.Copy(copy._intersectionToTile[i]!, entry, copy._intersectionToTile[i]!.Length);
                    _intersectionToTile.Add(entry);
                }
            }

            _edgeToWestTile.AddRange(copy._edgeToWestTile);
            _edgeToEastTile.AddRange(copy._edgeToEastTile);

            // Copy cache (not strictly necessary)
            for (int i = 0; i < copy._edgeToIntersection.Count; i++)
            {
                if (copy._edgeToIntersection[i] == null)
                    _edgeToIntersection.Add(null);
                else
                {
                    (Intersection top, Intersection bottom) = copy._edgeToIntersection[i]!.Value;
                    Intersection newTop = intersections[top.Index];
                    Intersection newBottom = intersections[bottom.Index];
                    _edgeToIntersection.Add((newTop, newBottom));
                }
            }

            for (int i = 0; i < copy._intersectionToEdge.Count; i++)
            {
                if (copy._intersectionToEdge[i] == null)
                    _intersectionToEdge.Add(null);
                else
                {
                    _intersectionToEdge.Add(copy._intersectionToEdge[i]!.Select(oldEdge => edges[oldEdge.Index]).ToHashSet());
                }
            }
        }

        /// <summary>
        /// Parameterless constructor for deserialization
        /// </summary>
        private AdjacencyMatrix()
        {
            // -- Cache --
            _edgeToIntersection = new List<(Intersection top, Intersection bottom)?>();
            _intersectionToEdge = new List<HashSet<Edge>?>();
        }

        /// <summary>
        /// Restore board references after deserialization
        /// </summary>
        /// <param name="board"></param>
        public void Attach(Board board)
        {
            if (Map != null || Intersections != null || Edges != null)
                throw new InvalidOperationException("AdjacencyMatrix is already attached to a board");

            Map = board.Map;
            Intersections = board.Intersections;
            Edges = board.Edges;
        }

        public void PrecomputeAll()
        {
            // Fill all caches by precomputing retrieval results
            for (int edgeIdx = 0; edgeIdx < Edges.Count; edgeIdx++)
            {
                _ = GetIntersections(Edges[edgeIdx]);
            }

            for (int intersectionIdx = 0; intersectionIdx < Intersections.Count; intersectionIdx++)
            {
                _ = GetEdges(Intersections[intersectionIdx]);
            }
        }

        public void Clear()
        {
            _tileToTile = new AdjIdx[Map.Width * Map.Height][];
            _tileToIntersection = new AdjIdx[Map.Width * Map.Height][];
            _tileToEdge = new AdjIdx[Map.Width * Map.Height][];
            _intersectionToTile = new List<AdjIdx[]?>();
            _edgeToWestTile = new List<AdjIdx>();
            _edgeToEastTile = new List<AdjIdx>();

            // -- Cache --
            _edgeToIntersection = new List<(Intersection top, Intersection bottom)?>();
            _intersectionToEdge = new List<HashSet<Edge>?>();
        }

        public void RegisterTile(Tile source, Direction.Tile direction, Tile neighbor)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);
            AdjIdx neighborIndex = GetIndexByTile(neighbor);

            if (_tileToTile[sourceIndex] == null)
            {
                _tileToTile[sourceIndex] = new AdjIdx[6];
                Array.Fill(_tileToTile[sourceIndex], NO_ADJACENCY);
            }

            _tileToTile[sourceIndex][(int)direction] = neighborIndex;
        }

        public void RegisterTile(Intersection source, Direction.Corner direction, Tile neighbor)
        {
            AdjIdx neighborIndex = GetIndexByTile(neighbor);

            // Add empty entries until the list is large enough to map the source intersection
            while (_intersectionToTile.Count <= source.Index)
            {
                _intersectionToTile.Add(null);
            }

            if (_intersectionToTile[source.Index] == null)
            {
                _intersectionToTile[source.Index] = new AdjIdx[6];
                Array.Fill(_intersectionToTile[source.Index]!, NO_ADJACENCY);
            }

            _intersectionToTile[source.Index]![(int)direction] = neighborIndex;
        }

        public void RegisterWestTile(Edge source, Tile neighbor)
        {
            AdjIdx neighborIndex = GetIndexByTile(neighbor);

            // Add empty entries until the list is large enough to map the source intersection
            while (_edgeToWestTile.Count <= source.Index)
            {
                _edgeToWestTile.Add(NO_ADJACENCY);
            }

            _edgeToWestTile[source.Index] = neighborIndex;
        }

        public void RegisterEastTile(Edge source, Tile neighbor)
        {
            AdjIdx neighborIndex = GetIndexByTile(neighbor);

            // Add empty entries until the list is large enough to map the source intersection
            while (_edgeToEastTile.Count <= source.Index)
            {
                _edgeToEastTile.Add(NO_ADJACENCY);
            }

            _edgeToEastTile[source.Index] = neighborIndex;
        }

        public void RegisterIntersection(Tile source, Direction.Corner direction, Intersection intersection)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);

            if (_tileToIntersection[sourceIndex] == null)
            {
                _tileToIntersection[sourceIndex] = new AdjIdx[6];
                Array.Fill(_tileToIntersection[sourceIndex], NO_ADJACENCY);
            }

            _tileToIntersection[sourceIndex][(int)direction] = (AdjIdx)intersection.Index;
        }

        public void RegisterEdge(Tile source, Direction.Tile direction, Edge edge)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);
            if (_tileToEdge[sourceIndex] == null)
            {
                _tileToEdge[sourceIndex] = new AdjIdx[6];
                Array.Fill(_tileToEdge[sourceIndex], NO_ADJACENCY);
            }
            _tileToEdge[sourceIndex][(int)direction] = (AdjIdx)edge.Index;
        }

        public Tile? GetTile(Tile source, Direction.Tile direction)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);

            if (_tileToTile[sourceIndex] == null)
                return null;

            AdjIdx neighborIndex = _tileToTile[sourceIndex][(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return GetTileByIndex(neighborIndex);
        }

        public Tile? GetTile(Intersection source, Direction.Corner direction)
        {
            if (_intersectionToTile[source.Index] == null)
                return null;

            AdjIdx neighborIndex = _intersectionToTile[source.Index]![(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return GetTileByIndex(neighborIndex);
        }

        public Tile? GetWestTile(Edge source)
        {
            if (_edgeToWestTile.Count <= source.Index)
                return null;

            if (_edgeToWestTile[source.Index] == NO_ADJACENCY)
                return null;

            return GetTileByIndex(_edgeToWestTile[source.Index]);
        }

        public Tile? GetEastTile(Edge source)
        {
            if (_edgeToEastTile.Count <= source.Index)
                return null;

            if (_edgeToEastTile[source.Index] == NO_ADJACENCY)
                return null;

            return GetTileByIndex(_edgeToEastTile[source.Index]);
        }

        public IEnumerable<Tile> GetTiles(Tile source)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);
            if (_tileToTile[sourceIndex] == null)
                yield break;

            foreach (AdjIdx neighborIndex in _tileToTile[sourceIndex])
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return GetTileByIndex(neighborIndex);
            }
        }

        public IEnumerable<Tile> GetTiles(Intersection source)
        {
            if (_intersectionToTile[source.Index] == null)
                yield break;

            foreach (AdjIdx neighborIndex in _intersectionToTile[source.Index]!)
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return GetTileByIndex(neighborIndex);
            }
        }

        public Intersection? GetIntersection(Tile source, Direction.Corner direction)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);

            if (_tileToIntersection[sourceIndex] == null)
                return null;

            AdjIdx neighborIndex = _tileToIntersection[sourceIndex][(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return Intersections[neighborIndex];
        }

        public IEnumerable<Intersection> GetIntersections(Tile source)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);
            if (_tileToIntersection[sourceIndex] == null)
                yield break;

            foreach (AdjIdx neighborIndex in _tileToIntersection[sourceIndex])
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return Intersections[neighborIndex];
            }
        }

        public (Intersection top, Intersection bottom) GetIntersections(Edge edge)
        {
            // Return cached value if it exists
            if (_edgeToIntersection.Count > edge.Index && _edgeToIntersection[edge.Index] != null)
            {
                return _edgeToIntersection[edge.Index]!.Value;
            }

            // Otherwise calculate result and cache
            while (_edgeToIntersection.Count <= edge.Index)
            {
                _edgeToIntersection.Add(null);
            }

            Tile? westTile = GetWestTile(edge);
            bool fromWest = westTile != null;
            Tile anchorTile = westTile ?? GetEastTile(edge)!;

            Direction.Tile tileDir = fromWest ? edge.Direction.ToEastTileDir() : edge.Direction.ToWestTileDir();
            (Direction.Corner left, Direction.Corner right) = tileDir.GetAdjacentCorners();

            Intersection leftIntersection = GetIntersection(anchorTile, left)!;
            Intersection rightIntersection = GetIntersection(anchorTile, right)!;

            (Intersection top, Intersection bottom) result = (fromWest ? leftIntersection : rightIntersection, fromWest ? rightIntersection : leftIntersection);
            _edgeToIntersection[edge.Index] = result;

            return result;
        }

        public Edge? GetEdge(Tile source, Direction.Tile direction)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);

            if (_tileToEdge[sourceIndex] == null)
                return null;

            AdjIdx neighborIndex = _tileToEdge[sourceIndex][(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return Edges[neighborIndex];
        }

        public IEnumerable<Edge> GetEdges(Tile source)
        {
            AdjIdx sourceIndex = GetIndexByTile(source);
            if (_tileToEdge[sourceIndex] == null)
                yield break;

            foreach (AdjIdx neighborIndex in _tileToEdge[sourceIndex])
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return Edges[neighborIndex];
            }
        }

        public HashSet<Edge> GetEdges(Intersection intersection)
        {
            // Return cached value if it exists
            if (_intersectionToEdge.Count > intersection.Index && _intersectionToEdge[intersection.Index] != null)
            {
                return _intersectionToEdge[intersection.Index]!;
            }

            // Otherwise calculate result and cache
            while (_intersectionToEdge.Count <= intersection.Index)
            {
                _intersectionToEdge.Add(null);
            }

            HashSet<Edge> result = new HashSet<Edge>();

            // Iterate through all adjacent tiles to the intersection (by corner direction)
            foreach (Direction.Corner anchoredDir in Enum.GetValues(typeof(Direction.Corner)))
            {
                Tile? tile = GetTile(intersection, anchoredDir);
                if (tile == null) continue;

                // Get the two roads adjacent to the tile and intersection
                (Direction.Tile leftAdjEdgeDir, Direction.Tile rightAdjEdgeDir) = anchoredDir.GetAdjacentTiles();
                Edge leftAdjEdge = GetEdge(tile!, leftAdjEdgeDir)!;
                Edge rightAdjEdge = GetEdge(tile!, rightAdjEdgeDir)!;

                result.Add(leftAdjEdge);
                result.Add(rightAdjEdge);
            }

            _intersectionToEdge[intersection.Index] = result;

            return result;
        }

        protected Tile GetTileByIndex(AdjIdx idx)
        {
            int x = (int)(idx % Map.Width);
            int y = (int)(idx / Map.Width);
            return Map.GetTile(x, y);
        }

        protected AdjIdx GetIndexByTile(Tile tile)
        {
            return (AdjIdx)(tile.Y * Map.Width + tile.X);
        }

        public override bool Equals(object? obj)
        {
            return obj is AdjacencyMatrix matrix
                && _tileToTile.SequenceEqual(matrix._tileToTile, _arrayEqualityComparer)
                && _tileToIntersection.SequenceEqual(matrix._tileToIntersection, _arrayEqualityComparer)
                && _tileToEdge.SequenceEqual(matrix._tileToEdge, _arrayEqualityComparer)
                && _intersectionToTile.SequenceEqual(matrix._intersectionToTile, _arrayEqualityComparer)
                && _edgeToWestTile.SequenceEqual(matrix._edgeToWestTile)
                && _edgeToEastTile.SequenceEqual(matrix._edgeToEastTile);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                _tileToTile.Aggregate(0, (acc, v) => v == null ? acc : HashCode.Combine(acc, v.Aggregate(0, (acc2, v2) => HashCode.Combine(acc2, v2.GetHashCode())))),
                _tileToIntersection.Aggregate(0, (acc, v) => v == null ? acc : HashCode.Combine(acc, v.Aggregate(0, (acc2, v2) => HashCode.Combine(acc2, v2.GetHashCode())))),
                _tileToEdge.Aggregate(0, (acc, v) => v == null ? acc : HashCode.Combine(acc, v.Aggregate(0, (acc2, v2) => HashCode.Combine(acc2, v2.GetHashCode())))),
                _intersectionToTile.Aggregate(0, (acc, v) => v == null ? acc : HashCode.Combine(acc, v.Aggregate(0, (acc2, v2) => HashCode.Combine(acc2, v2.GetHashCode())))),
                _edgeToWestTile.Aggregate(0, (acc, v) => HashCode.Combine(acc, v.GetHashCode())),
                _edgeToEastTile.Aggregate(0, (acc, v) => HashCode.Combine(acc, v.GetHashCode()))
            );
        }

        public sealed class Converter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return type == typeof(AdjacencyMatrix);
            }

            public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
            {
                // Expect mapping start
                parser.Expect<MappingStart>();

                var result = new AdjacencyMatrix();

                while (!parser.Accept<MappingEnd>())
                {
                    var key = parser.Expect<Scalar>().Value ?? string.Empty;
                    switch (key)
                    {
                        case "TileToTile":
                        case "_tileToTile":
                            result._tileToTile = (AdjIdx[][]?)rootDeserializer(typeof(AdjIdx[][]))!;
                            break;
                        case "TileToIntersection":
                        case "_tileToIntersection":
                            result._tileToIntersection = (AdjIdx[][]?)rootDeserializer(typeof(AdjIdx[][]))!;
                            break;
                        case "TileToEdge":
                        case "_tileToEdge":
                            result._tileToEdge = (AdjIdx[][]?)rootDeserializer(typeof(AdjIdx[][]))!;
                            break;
                        case "IntersectionToTile":
                        case "_intersectionToTile":
                            result._intersectionToTile = (List<AdjIdx[]?>?)rootDeserializer(typeof(List<AdjIdx[]?>))!;
                            break;
                        case "EdgeToWestTile":
                        case "_edgeToWestTile":
                            result._edgeToWestTile = (List<AdjIdx>?)rootDeserializer(typeof(List<AdjIdx>))!;
                            break;
                        case "EdgeToEastTile":
                        case "_edgeToEastTile":
                            result._edgeToEastTile = (List<AdjIdx>?)rootDeserializer(typeof(List<AdjIdx>))!;
                            break;
                        default:
                            // Unknown key: skip value
                            _ = rootDeserializer(typeof(object));
                            break;
                    }
                }

                parser.Expect<MappingEnd>();

                // Caches are not serialized
                // Instead, they will be calculated lazily by methods that rely on them

                return result;
            }


            public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
            {
                emitter.Emit(new MappingStart());

                AdjacencyMatrix matrix = (AdjacencyMatrix)value!;

                // Adjacency arrays and lists
                emitter.Emit(new Scalar("TileToTile"));
                serializer(matrix._tileToTile);

                emitter.Emit(new Scalar("TileToIntersection"));
                serializer(matrix._tileToIntersection);

                emitter.Emit(new Scalar("TileToEdge"));
                serializer(matrix._tileToEdge);

                emitter.Emit(new Scalar("IntersectionToTile"));
                serializer(matrix._intersectionToTile);

                emitter.Emit(new Scalar("EdgeToWestTile"));
                serializer(matrix._edgeToWestTile);

                emitter.Emit(new Scalar("EdgeToEastTile"));
                serializer(matrix._edgeToEastTile);

                emitter.Emit(new MappingEnd());
            }
        }
    }
}
