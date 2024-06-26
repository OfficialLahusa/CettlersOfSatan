﻿using Common;
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
        public bool DrawIntersectionMarkers = false;

        // Static geometry
        private VertexBuffer _tiles;
        private VertexArray _grid;
        private VertexArray _overlay;
        private VertexArray _portBridges;
        private VertexArray _roads;

        private Func<TileType, Color> _tileTypeColorFunc;
        private Func<Tile, Color?> _gridColorFunc;

        // Number tokens
        private Text _tokenText;
        private CircleShape _tokenBase;

        // Port text
        private Text _portText;

        private CircleShape _intersectionMarker;
        private RectangleShape _intersectionRect;

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
            _portBridges = new VertexArray(PrimitiveType.Triangles);
            _roads = new VertexArray(PrimitiveType.Triangles);

            _tokenText = new Text("", GameScreen.Font);
            _tokenText.CharacterSize = (uint)Math.Round(sideLength * (11.0f/30.0f));
            _tokenText.FillColor = Color.Black;

            _tokenBase = new CircleShape(SideLength/3, 64);
            _tokenBase.Origin = new Vector2f(_tokenBase.Radius, _tokenBase.Radius);

            _portText = new Text("", GameScreen.Font);
            _portText.CharacterSize = (uint)Math.Round(sideLength * (8.0f / 30.0f));
            _portText.FillColor = Color.White;

            _intersectionMarker = new CircleShape(SideLength / 6, 32);
            _intersectionMarker.Origin = new Vector2f(_intersectionMarker.Radius, _intersectionMarker.Radius);
            _intersectionMarker.FillColor = Color.Red;

            _intersectionRect = new RectangleShape(new Vector2f(SideLength * 0.5f, SideLength * 0.5f));
            _intersectionRect.Origin = new Vector2f(_intersectionRect.Size.X / 2, _intersectionRect.Size.Y / 2);
            _intersectionRect.Texture = TextureAtlas.Texture;

            _tileTypeColorFunc = val => val switch
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
            _portBridges.Clear();
            _roads.Clear();

            // Build tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile tile = Board.Map.GetTile(x, y);

                    // Calculate Color from Tile Value
                    Color tileColor = _tileTypeColorFunc(Board.Map.GetTile(x, y).Type);
                    Color? gridColor = _gridColorFunc(Board.Map.GetTile(x, y));

                    Vector2f center = new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);
                    
                    Vector2f[] points = new Vector2f[6];
                    Vertex[] vertices = new Vertex[6];

                    // Generate vertices
                    for (int i = 0; i < 6; i++)
                    {
                        // Hex points
                        points[i] = center + SideLength * ClientUtils.EulerAngleToVec2f(30.0f + 60.0f * i);

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
                    tempTiles[offset + 9]  = vertices[3];
                    tempTiles[offset + 10] = vertices[4];
                    tempTiles[offset + 11] = vertices[5];

                    // Build grid
                    if(gridColor.HasValue)
                    {
                        Vector2f[] insetPoints  = new Vector2f[6];
                        Vector2f[] outsetPoints = new Vector2f[6];
                        Vertex[] gridVertices       = new Vertex[6];
                        Vertex[] gridInsetVertices  = new Vertex[6];
                        Vertex[] gridOutsetVertices = new Vertex[6];

                        // Generate vertices
                        for(int i = 0; i < 6; i++)
                        {
                            // Inner points of hex (side length inset by half the border width)
                            insetPoints[i]  = center + (SideLength - _borderWidth / 2) * ClientUtils.EulerAngleToVec2f(30.0f + 60.0f * i);

                            // Outer points of hex (side length outset by half the border width)
                            outsetPoints[i] = center + (SideLength + _borderWidth / 2) * ClientUtils.EulerAngleToVec2f(30.0f + 60.0f * i);

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

                    // Build port bridges and icons
                    if(tile.Port != null)
                    {
                        Color portColor = new Color(0x65, 0x35, 0x0F);

                        // Coastal bridges
                        Direction.Corner leftCorner, rightCorner;
                        float leftAngle, rightAngle;

                        (leftCorner, rightCorner) = tile.Port.AnchorDirection.GetAdjacentCorners();
                        leftAngle = leftCorner.ToAngle();
                        rightAngle = rightCorner.ToAngle();

                        Vector2f leftDir = ClientUtils.EulerAngleToVec2f(leftAngle);
                        Vector2f rightDir = ClientUtils.EulerAngleToVec2f(rightAngle);

                        

                        Vertex vertLeftOuter  = new Vertex(center + (SideLength - _borderWidth / 2f) * leftDir,  portColor);
                        Vertex vertRightOuter = new Vertex(center + (SideLength - _borderWidth / 2f) * rightDir, portColor);
                        Vertex vertLeftInner  = new Vertex(center + SideLength * 0.75f * leftDir,  portColor);
                        Vertex vertRightInner = new Vertex(center + SideLength * 0.75f * rightDir, portColor);

                        _portBridges.Append(vertLeftOuter);
                        _portBridges.Append(vertRightOuter);
                        _portBridges.Append(vertLeftInner);

                        _portBridges.Append(vertLeftInner);
                        _portBridges.Append(vertRightOuter);
                        _portBridges.Append(vertRightInner);
                        

                        // Icons
                        float iconSize = SideLength * 7 / 15;

                        // Port icon (bottom)
                        IntRect textureRect = TextureAtlas.Sprite.Port.GetTextureRect();

                        Vector2f position = new Vector2f(center.X - iconSize / 2, center.Y - iconSize / 2 + (SideLength / 2.5f));
                        Vertex vertTopLeft     = new Vertex(position, portColor, new Vector2f(textureRect.Left, textureRect.Top));
                        Vertex vertTopRight    = new Vertex(position + new Vector2f(iconSize, 0), portColor, new Vector2f(textureRect.Left + textureRect.Width, textureRect.Top));
                        Vertex vertBottomLeft  = new Vertex(position + new Vector2f(0, iconSize), portColor, new Vector2f(textureRect.Left, textureRect.Top + textureRect.Height));
                        Vertex vertBottomRight = new Vertex(position + new Vector2f(iconSize, iconSize), portColor, new Vector2f(textureRect.Left + textureRect.Width, textureRect.Top + textureRect.Height));

                        _overlay.Append(vertTopLeft);
                        _overlay.Append(vertTopRight);
                        _overlay.Append(vertBottomLeft);
                        _overlay.Append(vertTopRight);
                        _overlay.Append(vertBottomRight);
                        _overlay.Append(vertBottomLeft);

                        // Type icon (top)
                        Color resourceColor;
                        byte max; float fac;
                        switch (tile.Port.Type)
                        {
                            case Port.TradeType.Lumber:
                                resourceColor = _tileTypeColorFunc(TileType.Lumber) * new Color(128, 128, 128);
                                textureRect = TextureAtlas.Sprite.Lumber.GetTextureRect();
                                break;
                            case Port.TradeType.Brick:
                                resourceColor = _tileTypeColorFunc(TileType.Brick);
                                max = Math.Max(resourceColor.R, Math.Max(resourceColor.G, resourceColor.B));
                                fac = 255.0f / max * 0.7f;
                                resourceColor *= new Color((byte)(resourceColor.R * fac), (byte)(resourceColor.G * fac), (byte)(resourceColor.B * fac));
                                textureRect = TextureAtlas.Sprite.Brick.GetTextureRect();
                                break;
                            case Port.TradeType.Wool:
                                resourceColor = new Color(230, 230, 230);
                                textureRect = TextureAtlas.Sprite.Wool.GetTextureRect();
                                break;
                            case Port.TradeType.Grain:
                                resourceColor = _tileTypeColorFunc(TileType.Grain);
                                max = Math.Max(resourceColor.R, Math.Max(resourceColor.G, resourceColor.B));
                                fac = 255.0f / max * 0.8f;
                                resourceColor *= new Color((byte)(resourceColor.R * fac), (byte)(resourceColor.G * fac), (byte)(resourceColor.B * fac));
                                textureRect = TextureAtlas.Sprite.Grain.GetTextureRect();
                                break;
                            case Port.TradeType.Ore:
                                resourceColor = _tileTypeColorFunc(TileType.Ore) * new Color(128, 128, 128);
                                textureRect = TextureAtlas.Sprite.Ore.GetTextureRect();
                                break;
                            case Port.TradeType.Generic:
                            default:
                                resourceColor = new Color(230, 230, 230);
                                textureRect = TextureAtlas.Sprite.QuestionMark.GetTextureRect();
                                break;
                        }

                        position = new Vector2f(center.X - iconSize / 2, center.Y - iconSize / 2 - (SideLength / 2.5f));
                        vertTopLeft = new Vertex(position, resourceColor, new Vector2f(textureRect.Left, textureRect.Top));
                        vertTopRight = new Vertex(position + new Vector2f(iconSize, 0), resourceColor, new Vector2f(textureRect.Left + textureRect.Width, textureRect.Top));
                        vertBottomLeft = new Vertex(position + new Vector2f(0, iconSize), resourceColor, new Vector2f(textureRect.Left, textureRect.Top + textureRect.Height));
                        vertBottomRight = new Vertex(position + new Vector2f(iconSize, iconSize), resourceColor, new Vector2f(textureRect.Left + textureRect.Width, textureRect.Top + textureRect.Height));

                        _overlay.Append(vertTopLeft);
                        _overlay.Append(vertTopRight);
                        _overlay.Append(vertBottomLeft);
                        _overlay.Append(vertTopRight);
                        _overlay.Append(vertBottomRight);
                        _overlay.Append(vertBottomLeft);
                    }

                    // Yield icons for land tiles
                    if(tile.IsLandTile())
                    {
                        Color color = _tileTypeColorFunc(tile.Type);

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

                        // Place icon above and below
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

            // Build roads
            foreach (Edge edge in Board.Edges)
            {
                // Skip edges without roads
                if (edge.Building == Edge.BuildingType.None) continue;

                // Anchor the edge to one of two neighboring tiles
                Tile anchor;
                Direction.Tile anchorDir;

                // Anchor to east tile, if it exists
                if (edge.EastTile != null)
                {
                    anchor = edge.EastTile;
                    anchorDir = edge.Direction.ToWestTileDir();
                }
                // Anchor to west tile otherwise
                else if (edge.WestTile != null)
                {
                    anchor = edge.WestTile;
                    anchorDir = edge.Direction.ToEastTileDir();
                }
                // If there are no adjacent tiles, the edge state must be invalid
                else
                {
                    throw new InvalidOperationException("Edge doesn't have adjacent tiles");
                }

                // Determine corner positions
                (Direction.Corner leftCorner, Direction.Corner rightCorner) = anchorDir.GetAdjacentCorners();
                float leftAngle  = leftCorner.ToAngle();
                float rightAngle = rightCorner.ToAngle();

                Vector2f tileCenter     = GetTileCenter(anchor);
                Vector2f leftDir        = ClientUtils.EulerAngleToVec2f(leftAngle);
                Vector2f rightDir       = ClientUtils.EulerAngleToVec2f(rightAngle);
                Vector2f leftInnerDir   = ClientUtils.EulerAngleToVec2f(leftCorner.Mirror().ToAngle());
                Vector2f rightInnerDir  = ClientUtils.EulerAngleToVec2f(rightCorner.Mirror().ToAngle());
                Vector2f leftOuterDir   = ClientUtils.EulerAngleToVec2f(leftCorner.Rotate(1).ToAngle());
                Vector2f rightOuterDir  = ClientUtils.EulerAngleToVec2f(rightCorner.Rotate(-1).ToAngle());

                Color playerColor = ClientUtils.GetPlayerColor(edge.Owner);

                // Build vertices
                Vertex left         = new Vertex(tileCenter + SideLength * leftDir, playerColor);
                Vertex right        = new Vertex(tileCenter + SideLength * rightDir, playerColor);
                Vertex leftInset    = new Vertex(left.Position  + leftInnerDir  * (_borderWidth / 2), playerColor);
                Vertex rightInset   = new Vertex(right.Position + rightInnerDir * (_borderWidth / 2), playerColor);
                Vertex leftOutset   = new Vertex(left.Position  + leftOuterDir  * (_borderWidth / 2), playerColor);
                Vertex rightOutset  = new Vertex(right.Position + rightOuterDir * (_borderWidth / 2), playerColor);


                _roads.Append(leftInset);
                _roads.Append(leftOutset);
                _roads.Append(rightInset);

                _roads.Append(leftOutset);
                _roads.Append(rightOutset);
                _roads.Append(rightInset);

                _roads.Append(left);
                _roads.Append(leftInset);
                _roads.Append(leftOutset);

                _roads.Append(right);
                _roads.Append(rightInset);
                _roads.Append(rightOutset);
            }
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

            return parentCenter + SideLength * ClientUtils.EulerAngleToVec2f(angle);
        }

        // Render the HexMap to a RenderTarget
        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_tiles, states);
            target.Draw(_grid, states);
            target.Draw(_portBridges, states);
            target.Draw(_roads, states);
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
                        _tokenText.DisplayedString = value.Number.ToString();
                        if (value.Number == 8 || value.Number == 6)
                        {
                            _tokenText.FillColor = Color.Red;
                            _tokenText.Style = Text.Styles.Bold;
                        }
                        else
                        {
                            _tokenText.FillColor = Color.Black;
                            _tokenText.Style = Text.Styles.Regular;
                        }
                        FloatRect bounds = _tokenText.GetLocalBounds();
                        _tokenText.Origin = new Vector2f(bounds.Left + bounds.Width / 2.0f, bounds.Top + bounds.Height / 2.0f);
                        _tokenText.Position = center;
                        target.Draw(_tokenText, states);
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

            // Draw intersection buildings
            foreach(Intersection intersection in Board.Intersections)
            {
                switch(intersection.Building)
                {
                    case Intersection.BuildingType.Settlement:
                    case Intersection.BuildingType.City:
                        _intersectionRect.Position = GetIntersectionCenter(intersection);
                        _intersectionRect.TextureRect = TextureAtlas.GetTextureRect(intersection.Building == Intersection.BuildingType.Settlement ? TextureAtlas.Sprite.Settlement : TextureAtlas.Sprite.City);
                        _intersectionRect.FillColor = ClientUtils.GetPlayerColor(intersection.Owner);
                        target.Draw(_intersectionRect, states);
                        break;
                    default:
                        break;
                }
            }

            // Draw port text
            foreach(Port port in Board.Ports)
            {
                Vector2f center = GetTileCenter(port.AnchorTile);
                _portText.DisplayedString = (port.Type == Port.TradeType.Generic) ? "3:1" : "2:1";
                FloatRect textBounds = _portText.GetLocalBounds();
                _portText.Origin = new Vector2f(textBounds.Left + textBounds.Width / 2f, textBounds.Top + textBounds.Height / 2f);
                _portText.Position = center + new Vector2f(-2, 0);

                target.Draw(_portText, states);
            }
        }
    }
}

