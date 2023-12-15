using Common;
using ImGuiNET;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace Client
{
    public class HexMapRenderer<T> : Drawable
    {
        public HexMap<T> Map { get; set; }

        private VertexBuffer _tiles;
        private VertexArray _grid;
        
        private Func<T, Color> _tileColorFunc;
        private Func<T, Color?> _gridColorFunc;

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

        public HexMapRenderer(HexMap<T> map, Func<T, Color> tileColorFunc, Func<T, Color?> gridColorFunc, float sideLength, float borderWidth)
        {
            Map = map;
            _tileColorFunc = tileColorFunc;
            _gridColorFunc = gridColorFunc;
            SideLength = sideLength;
            _borderWidth = borderWidth;

            _tiles = new VertexBuffer(Map.Width * Map.Height * 4 * 3, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Static);
            _grid = new VertexArray(PrimitiveType.Triangles);

            // Create Geometry
            Update();
        }

        // Update Tile Geometry
        public void Update()
        {
            uint width = Map.Width;
            uint height = Map.Height;
            Vertex[] tempTiles = new Vertex[width * height * 4 * 3];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate Color from Tile Value
                    Color tileColor = _tileColorFunc(Map.GetTile(x, y));
                    Color? gridColor = _gridColorFunc(Map.GetTile(x, y));

                    Vector2f center = new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);

                    
                    Vector2f[] points = new Vector2f[6];
                    Vertex[] vertices = new Vertex[6];

                    // Generate vertices
                    for (int i = 0; i < 6; i++)
                    {
                        // Hex points
                        points[i] = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI));

                        // Hex vertices
                        vertices[i] = new Vertex(points[i], tileColor);
                    }

                    // Appending vertices in groups of three in correct order for triangle drawing
                    int offset = (int)(x * height + y) * 4 * 3;
                    // Top triangle
                    tempTiles[offset + 0] = vertices[0];
                    tempTiles[offset + 1] = vertices[1];
                    tempTiles[offset + 2] = vertices[2];
                    // Middle top left triangle
                    tempTiles[offset + 3] = vertices[0];
                    tempTiles[offset + 4] = vertices[2];
                    tempTiles[offset + 5] = vertices[3];
                    // Middle bottom right triangle
                    tempTiles[offset + 6] = vertices[0];
                    tempTiles[offset + 7] = vertices[3];
                    tempTiles[offset + 8] = vertices[5];
                    // Bottom triangle
                    tempTiles[offset + 9] = vertices[3];
                    tempTiles[offset + 10] = vertices[4];
                    tempTiles[offset + 11] = vertices[5];

                    // Build grid
                    if(gridColor.HasValue)
                    {
                        Vector2f[] insetPoints = new Vector2f[6];
                        Vector2f[] outsetPoints = new Vector2f[6];
                        Vertex[] gridVertices = new Vertex[6];
                        Vertex[] gridInsetVertices = new Vertex[6];
                        Vertex[] gridOutsetVertices = new Vertex[6];

                        // Generate vertices
                        for(int i = 0; i < 6; i++)
                        {
                            // Inner points of hex (side length inset by half the border width)
                            insetPoints[i] = center + (SideLength - _borderWidth / 2) * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI));

                            // Outer points of hex (side length outset by half the border width)
                            outsetPoints[i] = center + (SideLength + _borderWidth / 2) * new Vector2f(MathF.Cos((30.0f + 60.0f * i) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * i) / 180.0f * MathF.PI));

                            // Grid vertices
                            gridVertices[i] = new Vertex(points[i], gridColor.Value);
                            gridInsetVertices[i] = new Vertex(insetPoints[i], gridColor.Value);
                            gridOutsetVertices[i] = new Vertex(outsetPoints[i], gridColor.Value);
                        }

                        // Append grid triangles
                        for(int i = 0; i < 6; i++)
                        {
                            // Outer Triangle
                            _grid.Append(gridOutsetVertices[i]);
                            _grid.Append(gridOutsetVertices[(i + 1) % 6]);
                            _grid.Append(gridInsetVertices[i]);

                            // Inner Triangle
                            _grid.Append(gridInsetVertices[i]);
                            _grid.Append(gridInsetVertices[(i + 1) % 6]);
                            _grid.Append(gridOutsetVertices[(i + 1) % 6]);
                        }
                    }
                }
            }
            _tiles.Update(tempTiles);
        }

        // Get the Coordinates of the Center of a Given Tile Relative to the Origin of the HexMap
        public Vector2f GetTileCenter(int x, int y)
        {
            if (x >= Map.Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            else if (y >= Map.Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");
            else
            {
                return new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);
            }
        }

        // Render the HexMap to a RenderTarget
        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_tiles, states);
            target.Draw(_grid, states);
        }
    }
}

