using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class AdjacencyMatrix
    {
        // TODO: Equals, GetHashCode, Clear, Copy constructor

        protected const int NO_ADJACENCY = -1;

        protected HexMap<Tile> _map;
        protected List<Intersection> _intersections;
        protected List<Edge> _edges;

        // Dim 1: Source tile
        // Dim 2: Tile direction
        // Stored: Flattened index of adjacent tile (y * width + x)
        protected int[][] _tileToTile;

        // Dim 1: Source tile
        // Dim 2: Corner direction
        // Stored: Index of adjacent intersection
        protected int[][] _tileToIntersection;

        // Dim 1: Source tile
        // Dim 2: Tile direction
        // Stored: Index of adjacent edge
        protected int[][] _tileToEdge;

        // Dim 1: Source intersection
        // Dim 2: Corner direction (that the source intersection is on at the adjacent tile)
        // Stored: Tile index of adjacent tile
        protected List<int[]?> _intersectionToTile;

        // Dim 1: Source edge
        // Stored: Tile index of adjacent tile
        protected List<int> _edgeToWestTile;

        // Dim 1: Source edge
        // Stored: Tile index of adjacent tile
        protected List<int> _edgeToEastTile;

        public AdjacencyMatrix(HexMap<Tile> map, List<Intersection> intersections, List<Edge> edges)
        {
            _map = map;
            _intersections = intersections;
            _edges = edges;

            _tileToTile = new int[_map.Width * _map.Height][];
            _tileToIntersection = new int[_map.Width * _map.Height][];
            _tileToEdge = new int[_map.Width * _map.Height][];
            _intersectionToTile = new List<int[]?>();
            _edgeToWestTile = new List<int>();
            _edgeToEastTile = new List<int>();
        }

        public void RegisterTile(Tile source, Direction.Tile direction, Tile neighbor)
        {
            int sourceIndex = GetIndexByTile(source);
            int neighborIndex = GetIndexByTile(neighbor);

            if (_tileToTile[sourceIndex] == null)
            {
                _tileToTile[sourceIndex] = new int[6];
                Array.Fill(_tileToTile[sourceIndex], NO_ADJACENCY);
            }

            _tileToTile[sourceIndex][(int)direction] = neighborIndex;
        }

        public void RegisterTile(Intersection source, Direction.Corner direction, Tile neighbor)
        {
            int neighborIndex = GetIndexByTile(neighbor);

            // Add empty entries until the list is large enough to map the source intersection
            while (_intersectionToTile.Count <= source.Index)
            {
                _intersectionToTile.Add(null);
            }

            if (_intersectionToTile[source.Index] == null)
            {
                _intersectionToTile[source.Index] = new int[6];
                Array.Fill(_intersectionToTile[source.Index]!, NO_ADJACENCY);
            }

            _intersectionToTile[source.Index]![(int)direction] = neighborIndex;
        }

        public void RegisterWestTile(Edge source, Tile neighbor)
        {
            int neighborIndex = GetIndexByTile(neighbor);

            // Add empty entries until the list is large enough to map the source intersection
            while (_edgeToWestTile.Count <= source.Index)
            {
                _edgeToWestTile.Add(NO_ADJACENCY);
            }

            _edgeToWestTile[source.Index] = neighborIndex;
        }

        public void RegisterEastTile(Edge source, Tile neighbor)
        {
            int neighborIndex = GetIndexByTile(neighbor);

            // Add empty entries until the list is large enough to map the source intersection
            while (_edgeToEastTile.Count <= source.Index)
            {
                _edgeToEastTile.Add(NO_ADJACENCY);
            }

            _edgeToEastTile[source.Index] = neighborIndex;
        }

        public void RegisterIntersection(Tile source, Direction.Corner direction, Intersection intersection)
        {
            int sourceIndex = GetIndexByTile(source);

            if (_tileToIntersection[sourceIndex] == null)
            {
                _tileToIntersection[sourceIndex] = new int[6];
                Array.Fill(_tileToIntersection[sourceIndex], NO_ADJACENCY);
            }

            _tileToIntersection[sourceIndex][(int)direction] = intersection.Index;
        }

        public void RegisterEdge(Tile source, Direction.Tile direction, Edge edge)
        {
            int sourceIndex = GetIndexByTile(source);
            if (_tileToEdge[sourceIndex] == null)
            {
                _tileToEdge[sourceIndex] = new int[6];
                Array.Fill(_tileToEdge[sourceIndex], NO_ADJACENCY);
            }
            _tileToEdge[sourceIndex][(int)direction] = edge.Index;
        }

        public Tile? GetTile(Tile source, Direction.Tile direction)
        {
            int sourceIndex = GetIndexByTile(source);

            if (_tileToTile[sourceIndex] == null)
                return null;

            int neighborIndex = _tileToTile[sourceIndex][(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return GetTileByIndex(neighborIndex);
        }

        public Tile? GetTile(Intersection source, Direction.Corner direction)
        {
            if (_intersectionToTile[source.Index] == null)
                return null;

            int neighborIndex = _intersectionToTile[source.Index]![(int)direction];
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
            int sourceIndex = GetIndexByTile(source);
            if (_tileToTile[sourceIndex] == null)
                yield break;

            foreach (int neighborIndex in _tileToTile[sourceIndex])
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return GetTileByIndex(neighborIndex);
            }
        }

        public IEnumerable<Tile> GetTiles(Intersection source)
        {
            if (_intersectionToTile[source.Index] == null)
                yield break;

            foreach (int neighborIndex in _intersectionToTile[source.Index]!)
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return GetTileByIndex(neighborIndex);
            }
        }

        public Intersection? GetIntersection(Tile source, Direction.Corner direction)
        {
            int sourceIndex = GetIndexByTile(source);

            if (_tileToIntersection[sourceIndex] == null)
                return null;

            int neighborIndex = _tileToIntersection[sourceIndex][(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return _intersections[neighborIndex];
        }

        public IEnumerable<Intersection> GetIntersections(Tile source)
        {
            int sourceIndex = GetIndexByTile(source);
            if (_tileToIntersection[sourceIndex] == null)
                yield break;

            foreach (int neighborIndex in _tileToIntersection[sourceIndex])
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return _intersections[neighborIndex];
            }
        }

        // TODO: Cachen
        public (Intersection top, Intersection bottom) GetIntersections(Edge edge)
        {
            Tile? westTile = GetWestTile(edge);
            bool fromWest = westTile != null;
            Tile anchorTile = westTile ?? GetEastTile(edge)!;

            Direction.Tile tileDir = fromWest ? edge.Direction.ToEastTileDir() : edge.Direction.ToWestTileDir();
            (Direction.Corner left, Direction.Corner right) = tileDir.GetAdjacentCorners();

            Intersection leftIntersection = GetIntersection(anchorTile, left)!;
            Intersection rightIntersection = GetIntersection(anchorTile, right)!;

            return (fromWest ? leftIntersection : rightIntersection, fromWest ? rightIntersection : leftIntersection);
        }

        public Edge? GetEdge(Tile source, Direction.Tile direction)
        {
            int sourceIndex = GetIndexByTile(source);

            if (_tileToEdge[sourceIndex] == null)
                return null;

            int neighborIndex = _tileToEdge[sourceIndex][(int)direction];
            if (neighborIndex == NO_ADJACENCY)
                return null;

            return _edges[neighborIndex];
        }

        public IEnumerable<Edge> GetEdges(Tile source)
        {
            int sourceIndex = GetIndexByTile(source);
            if (_tileToEdge[sourceIndex] == null)
                yield break;

            foreach (int neighborIndex in _tileToEdge[sourceIndex])
            {
                if (neighborIndex != NO_ADJACENCY)
                    yield return _edges[neighborIndex];
            }
        }

        // TODO: Cachen
        public HashSet<Edge> GetEdges(Intersection intersection)
        {
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

            return result;
        }

        protected Tile GetTileByIndex(int idx)
        {
            int x = (int)(idx % _map.Width);
            int y = (int)(idx / _map.Width);
            return _map.GetTile(x, y);
        }

        protected int GetIndexByTile(Tile tile)
        {
            return (int)(tile.Y * _map.Width + tile.X);
        }
    }
}
