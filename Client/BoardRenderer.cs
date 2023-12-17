using Common;
using SFML.Graphics;
using SFML.System;
using static Common.Tile;
using Color = SFML.Graphics.Color;

namespace Client
{
    public class BoardRenderer : Drawable
    {
        public Board Board { get; set; }

        // Rendering options
        public bool DrawTokenShadows = true;
        public bool DrawIntersectionMarkers = true;

        // Static geometry
        private VertexBuffer _tiles;
        private VertexArray _grid;
        private VertexArray _overlay;
        
        private Func<Tile, Color> _tileColorFunc;
        private Func<Tile, Color?> _gridColorFunc;

        private Text _coords;
        private CircleShape _tokenBase;
        private CircleShape _intersectionMarker;

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

        public BoardRenderer(Board board, float sideLength, float borderWidth)
        {
            Board = board;
            SideLength = sideLength;
            _borderWidth = borderWidth;

            _tiles = new VertexBuffer(Board.Map.Width * Board.Map.Height * 4 * 3, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Static);
            _grid = new VertexArray(PrimitiveType.Triangles);
            _overlay = new VertexArray(PrimitiveType.Triangles);

            _coords = new Text("", GameScreen.Font);
            _coords.CharacterSize = (uint)Math.Round(sideLength * (11.0f/30.0f));
            _coords.FillColor = Color.Black;

            _tokenBase = new CircleShape(SideLength/3, 64);
            _tokenBase.Origin = new Vector2f(_tokenBase.Radius, _tokenBase.Radius);

            _intersectionMarker = new CircleShape(SideLength / 6, 32);
            _intersectionMarker.Origin = new Vector2f(_intersectionMarker.Radius, _intersectionMarker.Radius);
            _intersectionMarker.FillColor = Color.Red;

            _tileColorFunc = val => val.Type switch
            {
                TileType.Water => new Color(0x2d, 0x64, 0x9d), // dark blue
                TileType.Lumber => new Color(0x44, 0x92, 0x47), // dark green
                TileType.Brick => new Color(0xd1, 0x70, 0x40), // orange-red
                TileType.Wool => new Color(0x96, 0xb1, 0x41), //light green
                TileType.Grain => new Color(0xe9, 0xbb, 0x4e), // yellow
                TileType.Ore => new Color(0xa5, 0xaa, 0xa7), // gray
                TileType.Desert => new Color(0xd6, 0xcf, 0x9d), // beige
                _ => Color.Transparent // non-playable => transparent
            };
            _gridColorFunc = val => val.Type switch
            {
                TileType.Water or TileType.NonPlayable => null, // transparent for water/non-playable
                _ => new Color(0xd5, 0xbe, 0x84) // white for land tiles
            };

            // Create Geometry
            Update();
        }

        // Update Tile Geometry
        public void Update()
        {
            uint width = Board.Map.Width;
            uint height = Board.Map.Height;
            Vertex[] tempTiles = new Vertex[width * height * 4 * 3];
            _grid.Clear();
            _overlay.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile tile = Board.Map.GetTile(x, y);

                    // Calculate Color from Tile Value
                    Color tileColor = _tileColorFunc(Board.Map.GetTile(x, y));
                    Color? gridColor = _gridColorFunc(Board.Map.GetTile(x, y));

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
                        Color color = _tileColorFunc(tile);

                        // Grain/Brick: Increase primary intensity
                        if(tile.Type == TileType.Grain || tile.Type == TileType.Brick)
                        {
                            byte max = Math.Max(color.R, Math.Max(color.G, color.B));
                            float fac = 255.0f / max * ((tile.Type == TileType.Grain) ? 0.8f : 0.7f);
                            color *= new Color((byte)(color.R * fac), (byte)(color.G * fac), (byte)(color.B * fac));
                        }
                        // Wool: Static light gray/white shade
                        else if (tile.Type == TileType.Wool)
                        {
                            color = new Color(230, 230, 230);
                        }
                        // Ore/Lumber/Desert: Reduce color intensity by 50%
                        else
                        {
                            color *= new Color(128, 128, 128);
                        }

                        IntRect textureRect = TextureAtlas.GetSprite(tile.Type).GetTextureRect();
                        float iconSize = SideLength * 7 / 15;

                        for(int i = -1; i < 2; i += 2)
                        {
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

        public Vector2f GetTileCenter(Tile tile)
        {
            return GetTileCenter(tile.X, tile.Y);
        }

        // Get the Coordinates of the Center of a Given Tile Relative to the Origin of the HexMap
        public Vector2f GetTileCenter(int x, int y)
        {
            if (x >= Board.Map.Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            else if (y >= Board.Map.Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");
            else
            {
                return new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);
            }
        }

        public Vector2f GetIntersectionCenter(Intersection intersection)
        {
            if (intersection.AdjacentTiles.Count == 0)
            {
                throw new InvalidOperationException("Intersection was not properly initialized and lacks adjacent tiles");
            }

            KeyValuePair<Direction.Corner, Tile> adjacentParent = intersection.AdjacentTiles.First();
            Vector2f parentCenter = GetTileCenter(adjacentParent.Value);
            float angle = adjacentParent.Key.ToAngle();

            return parentCenter + SideLength * new Vector2f((float)Math.Cos(angle / 180.0f * MathF.PI), (float)Math.Sin(angle / 180.0f * MathF.PI));
        }

        // Render the HexMap to a RenderTarget
        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_tiles, states);
            target.Draw(_grid, states);
            states.Texture = TextureAtlas.Texture;
            target.Draw(_overlay, states);

            // Draw number tokens
            for (int y = 0; y < Board.Map.Height; y++)
            {
                for (int x = 0; x < Board.Map.Width; x++)
                {
                    Tile value = Board.Map.GetTile(x, y);
                    if (value.HasYield())
                    {
                        // Token Base Circle
                        Vector2f center = GetTileCenter(x, y);
                        // Shadow
                        if(DrawTokenShadows)
                        {
                            _tokenBase.FillColor = new Color(0, 0, 0, 75);
                            _tokenBase.Position = ClientUtils.RoundVec2f(center + new Vector2f(3, 3));
                            target.Draw(_tokenBase, states);
                        }
                        // Token
                        _tokenBase.FillColor = new Color(230, 230, 120);
                        _tokenBase.Position = ClientUtils.RoundVec2f(center);
                        target.Draw(_tokenBase, states);

                        // Yield Text (Most frequent numbers 8, 6 are red)
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

            // Draw intersection markers
            if (DrawIntersectionMarkers)
            {
                foreach (Intersection intersection in Board.Intersections)
                {
                    _intersectionMarker.Position = GetIntersectionCenter(intersection);
                    target.Draw(_intersectionMarker, states);
                }
            }
        }
    }
}

