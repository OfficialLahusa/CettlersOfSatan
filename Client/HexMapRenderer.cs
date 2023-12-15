using Common;
using SFML.Graphics;
using SFML.System;
using Color = SFML.Graphics.Color;

namespace Client
{
    public class HexMapRenderer<T> : Drawable
    {
        private VertexBuffer _tiles;
        private VertexArray _gridLines;
        private HexMap<T> _map;
        private Func<T, Color> _tileColorFunc;
        private Func<T, Color> _gridColorFunc;

        #region Hexagon Dimensions
        private float _sideLength;
        private float _borderWidth;

        // Outer circle radius
        public float SideLength
        {
            get => _sideLength;
            set
            {
                _sideLength = value;
                FlatSideLength = (MathF.Sqrt(3.0f) / 2.0f) * _sideLength;
            }
        }

        // Inner circle radius (calculated automatically through property "SideLength", do not set manually)
        public float FlatSideLength
        {
            get;
            private set;
        }
        #endregion

        public HexMapRenderer(HexMap<T> map, Func<T, Color> tileColorFunc, Func<T, Color> gridColorFunc, float sideLength, float borderWidth)
        {
            _map = map;
            _tileColorFunc = tileColorFunc;
            _gridColorFunc = gridColorFunc;
            SideLength = sideLength;
            _borderWidth = borderWidth;

            _tiles = new VertexBuffer(_map.Width * _map.Height * 2 * 4 * 3, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Static);
            _gridLines = new VertexArray(PrimitiveType.Lines);

            // Create Geometry
            Update();
        }

        // Update a Single Tile 
        public void UpdateAt(int x, int y)
        {
            if (x >= _map.Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            if (y >= _map.Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");

            // Calculate Color from Tile Value
            Color color = _tileColorFunc(_map.GetTile(x, y));

            // Temporary Array of Partitioning Triangles, Which Will Be Later Used to Update the Vertex Buffer
            Vertex[] triangles = new Vertex[4 * 3];
            // Individual Vertices of the Hexagonal Tile
            Vertex[] hexTileVertices = new Vertex[6];

            // Center Point of the Hexagonal Tile
            Vector2f center = GetTileCenter(x, y);

            // Individual Hex Tile Vertices
            for (int i = 0; i < 6; i++)
            {
                hexTileVertices[i] = new Vertex(center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI)), color);
            }

            // Assigning Vertices in Groups of Three in Correct Order for Triangle Drawing
            // Top Triangle
            triangles[0] = hexTileVertices[0];
            triangles[1] = hexTileVertices[1];
            triangles[2] = hexTileVertices[2];
            // Middle Left Triangle
            triangles[3] = hexTileVertices[0];
            triangles[4] = hexTileVertices[2];
            triangles[5] = hexTileVertices[3];
            // Middle Right Triangle
            triangles[6] = hexTileVertices[0];
            triangles[7] = hexTileVertices[3];
            triangles[8] = hexTileVertices[5];
            // Bottom Triangle
            triangles[9] = hexTileVertices[3];
            triangles[10] = hexTileVertices[4];
            triangles[11] = hexTileVertices[5];

            // Update Vertex Buffer
            _tiles.Update(triangles, (uint)triangles.Length, (uint)(x * _map.Height + y) * 2 * 4 * 3);
        }

        // Update Tile Geometry
        public void Update()
        {
            uint width = _map.Width;
            uint height = _map.Height;
            Vertex[] tempTiles = new Vertex[width * height * 2 * 4 * 3];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate Color from Tile Value
                    Color tileColor = _tileColorFunc(_map.GetTile(x, y));
                    Color gridColor = _gridColorFunc(_map.GetTile(x, y));

                    Vector2f center = new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);

                    
                    Vector2f[] points = new Vector2f[6];
                    Vector2f[] insetPoints = new Vector2f[6];
                    Vector2f[] outsetPoints = new Vector2f[6];
                    Vertex[] vertices = new Vertex[6];
                    Vertex[] gridVertices = new Vertex[6];
                    for(int i = 0; i < 6; i++)
                    {
                        // Hex points
                        points[i] = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI));

                        // Inner points of hex (side length inset by half the border width)
                        insetPoints[i] = center + (SideLength - _borderWidth / 2) * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI));

                        // Outer points of hex (side length outset by half the border width)
                        outsetPoints[i] = center + (SideLength + _borderWidth / 2) * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI));

                        // Hex vertices
                        vertices[i] = new Vertex(points[i], tileColor);
                        // Grid vertices
                        gridVertices[i] = new Vertex(points[i], gridColor);
                    }

                    // Appending vertices in groups of three in correct order for triangle drawing
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 0] = vertices[0];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 1] = vertices[1];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 2] = vertices[2];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 3] = vertices[0];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 4] = vertices[2];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 5] = vertices[3];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 6] = vertices[0];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 7] = vertices[3];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 8] = vertices[5];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 9] = vertices[3];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 10] = vertices[4];
                    tempTiles[(x * height + y) * 2 * 4 * 3 + 11] = vertices[5];

                    // Appending vertices in pairs of two in correct order for line primitive type drawing
                    _gridLines.Append(gridVertices[0]);
                    _gridLines.Append(gridVertices[1]);
                    _gridLines.Append(gridVertices[1]);
                    _gridLines.Append(gridVertices[2]);
                    _gridLines.Append(gridVertices[2]);
                    _gridLines.Append(gridVertices[3]);
                    _gridLines.Append(gridVertices[3]);
                    _gridLines.Append(gridVertices[4]);
                    _gridLines.Append(gridVertices[4]);
                    _gridLines.Append(gridVertices[5]);
                    _gridLines.Append(gridVertices[5]);
                    _gridLines.Append(gridVertices[0]);
                }
            }
            _tiles.Update(tempTiles);
        }

        // Get the Coordinates of the Center of a Given Tile Relative to the Origin of the HexMap
        public Vector2f GetTileCenter(int x, int y)
        {
            if (x >= _map.Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            else if (y >= _map.Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");
            else
            {
                return new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);
            }
        }

        // Render the HexMap to a RenderTarget
        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_tiles, states);
            target.Draw(_gridLines, states);
        }
    }
}

