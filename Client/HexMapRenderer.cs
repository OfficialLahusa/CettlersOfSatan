using Common;
using ImGuiNET;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace Client
{
    public class HexMapRenderer : Drawable
    {
        public HexMap<Tile> Map { get; set; }

        private VertexBuffer _tiles;
        private VertexArray _grid;
        private VertexArray _overlay;
        
        private Func<Tile, Color> _tileColorFunc;
        private Func<Tile, Color?> _gridColorFunc;

        private Text _coords;
        private CircleShape _center;

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

        public HexMapRenderer(HexMap<Tile> map, Func<Tile, Color> tileColorFunc, Func<Tile, Color?> gridColorFunc, float sideLength, float borderWidth)
        {
            Map = map;
            _tileColorFunc = tileColorFunc;
            _gridColorFunc = gridColorFunc;
            SideLength = sideLength;
            _borderWidth = borderWidth;

            _tiles = new VertexBuffer(Map.Width * Map.Height * 4 * 3, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Static);
            _grid = new VertexArray(PrimitiveType.Triangles);
            _overlay = new VertexArray(PrimitiveType.Triangles);

            _coords = new Text("", GameScreen.Font);
            _coords.CharacterSize = 55;
            _coords.FillColor = Color.Black;
            _center = new CircleShape(50, 32);
            _center.FillColor = new Color(230, 230, 120);
            _center.Origin = new Vector2f(_center.Radius, _center.Radius);

            // Create Geometry
            Update();
        }

        // Update Tile Geometry
        public void Update()
        {
            uint width = Map.Width;
            uint height = Map.Height;
            Vertex[] tempTiles = new Vertex[width * height * 4 * 3];
            _grid.Clear();
            _overlay.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile tile = Map.GetTile(x, y);

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

                    // Build static overlay
                    if(tile.IsLandTile())
                    {
                        int texId = tile.Type switch
                        {
                            TileType.Lumber => 0,
                            TileType.Brick => 1,
                            TileType.Wool => 2,
                            TileType.Grain => 3,
                            TileType.Ore => 4,
                            TileType.Desert => 12,
                            _ => -1
                        };

                        // Reduce color intensity by 50%
                        Color color = _tileColorFunc(tile) * new Color(128, 128, 128);
                        FloatRect textureRect = (texId != -1) ? new FloatRect((texId % 8) * 512, (texId / 8) * 512, 512, 512) : new FloatRect(0, 0, 0, 0);
                        const float iconSize = 70;

                        for(int i = -1; i < 2; i += 2)
                        {
                            Console.WriteLine(i);
                            Vector2f position = new Vector2f(center.X - iconSize / 2, center.Y - iconSize / 2 + i * (SideLength / 1.75f));
                            Vertex vertTL = new Vertex(position, color, new Vector2f(textureRect.Left, textureRect.Top));
                            Vertex vertTR = new Vertex(position + new Vector2f(iconSize, 0), color, new Vector2f(textureRect.Left + textureRect.Width, textureRect.Top));
                            Vertex vertBL = new Vertex(position + new Vector2f(0, iconSize), color, new Vector2f(textureRect.Left, textureRect.Top + textureRect.Height));
                            Vertex vertBR = new Vertex(position + new Vector2f(iconSize, iconSize), color, new Vector2f(textureRect.Left + textureRect.Width, textureRect.Top + textureRect.Height));

                            _overlay.Append(vertTL);
                            _overlay.Append(vertTR);
                            _overlay.Append(vertBL);
                            _overlay.Append(vertTR);
                            _overlay.Append(vertBR);
                            _overlay.Append(vertBL);
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
            states.Texture = GameScreen.Atlas;
            target.Draw(_overlay, states);

            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    Tile value = Map.GetTile(x, y);
                    if (value.HasYield())
                    {
                        // Circle Base
                        Vector2f center = GetTileCenter(x, y);
                        _center.Position = center;
                        target.Draw(_center, states);

                        // Yield Text
                        _coords.DisplayedString = value.Number.ToString();
                        if (value.Number == 8 || value.Number == 6)
                        {
                            _coords.FillColor = Color.Red;
                            _coords.Style = Text.Styles.Bold;
                        }
                        else
                        {
                            _coords.FillColor = Color.Black;
                            _coords.Style = Text.Styles.Regular;
                        }
                        FloatRect bounds = _coords.GetLocalBounds();
                        _coords.Origin = new Vector2f(bounds.Left + bounds.Width / 2.0f, bounds.Top + bounds.Height / 2.0f);
                        _coords.Position = center;
                        target.Draw(_coords, states);
                    }
                }
            }
        }
    }
}

