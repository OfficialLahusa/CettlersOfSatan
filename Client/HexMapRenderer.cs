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
        private Func<T?, Color> _colorFunc;

        #region Hexagon Side Length Constants
        private float _sideLength;

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

        public HexMapRenderer(HexMap<T> map, Func<T?, Color> colorFunc, float sideLength)
        {
            _map = map;
            _colorFunc = colorFunc;
            SideLength = sideLength;

            _tiles = new VertexBuffer(_map.Width * _map.Height * 6 * 3, PrimitiveType.Triangles, VertexBuffer.UsageSpecifier.Static);
            _gridLines = new VertexArray(PrimitiveType.Lines);

            // Create All Polygons
            Rebuild();
        }

        // Update a Single Tile 
        public void RebuildOne(int x, int y)
        {
            if (x >= _map.Width || x < 0) throw new ArgumentOutOfRangeException("x", "x needs to be 0 <= x < Width");
            if (y >= _map.Height || y < 0) throw new ArgumentOutOfRangeException("y", "y needs to be 0 <= y < Height");

            // Calculate Color from Tile Value
            Color color = _colorFunc(_map.GetTile(x, y));

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
            _tiles.Update(triangles, (uint)triangles.Length, (uint)(x * _map.Height + y) * 4 * 3);
        }

        // Update Tile Geometry
        public void Rebuild()
        {
            uint width = _map.Width;
            uint height = _map.Height;
            Vertex[] tempTiles = new Vertex[width * height * 6 * 3];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate Color from Tile Value
                    Color color = _colorFunc(_map.GetTile(x, y));

                    Vector2f center = new Vector2f(x * 2 * FlatSideLength + ((y % 2 == 0) ? FlatSideLength : 0), y * 1.5f * SideLength);

                    // Define edge points
                    Vector2f point1 = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * 0) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * 0) / 180.0f * MathF.PI));
                    Vector2f point2 = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * 1) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * 1) / 180.0f * MathF.PI));
                    Vector2f point3 = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * 2) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * 2) / 180.0f * MathF.PI));
                    Vector2f point4 = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * 3) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * 3) / 180.0f * MathF.PI));
                    Vector2f point5 = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * 4) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * 4) / 180.0f * MathF.PI));
                    Vector2f point6 = center + SideLength * new Vector2f(MathF.Cos((30.0f + 60.0f * 5) / 180.0f * MathF.PI), MathF.Sin((30.0f + 60.0f * 5) / 180.0f * MathF.PI));

                    #region Hexagon Filling
                    // Tile partition triangle vertices
                    Vertex tileVertex1 = new Vertex(point1, color);
                    Vertex tileVertex2 = new Vertex(point2, color);
                    Vertex tileVertex3 = new Vertex(point3, color);
                    Vertex tileVertex4 = new Vertex(point4, color);
                    Vertex tileVertex5 = new Vertex(point5, color);
                    Vertex tileVertex6 = new Vertex(point6, color);

                    // Appending vertices in groups of three in correct order for triangle drawing
                    tempTiles[(x * height + y) * 4 * 3 + 0] = tileVertex1;
                    tempTiles[(x * height + y) * 4 * 3 + 1] = tileVertex2;
                    tempTiles[(x * height + y) * 4 * 3 + 2] = tileVertex3;
                    tempTiles[(x * height + y) * 4 * 3 + 3] = tileVertex1;
                    tempTiles[(x * height + y) * 4 * 3 + 4] = tileVertex3;
                    tempTiles[(x * height + y) * 4 * 3 + 5] = tileVertex4;
                    tempTiles[(x * height + y) * 4 * 3 + 6] = tileVertex1;
                    tempTiles[(x * height + y) * 4 * 3 + 7] = tileVertex4;
                    tempTiles[(x * height + y) * 4 * 3 + 8] = tileVertex6;
                    tempTiles[(x * height + y) * 4 * 3 + 9] = tileVertex4;
                    tempTiles[(x * height + y) * 4 * 3 + 10] = tileVertex5;
                    tempTiles[(x * height + y) * 4 * 3 + 11] = tileVertex6;
                    #endregion

                    #region Hexagon GridLines
                    // Grid line vertices
                    Vertex gridVertex1 = new Vertex(point1, Color.White);
                    Vertex gridVertex2 = new Vertex(point2, Color.White);
                    Vertex gridVertex3 = new Vertex(point3, Color.White);
                    Vertex gridVertex4 = new Vertex(point4, Color.White);
                    Vertex gridVertex5 = new Vertex(point5, Color.White);
                    Vertex gridVertex6 = new Vertex(point6, Color.White);

                    // Appending vertices in pairs of two in correct order for line primitive type drawing
                    _gridLines.Append(gridVertex1);
                    _gridLines.Append(gridVertex2);
                    _gridLines.Append(gridVertex2);
                    _gridLines.Append(gridVertex3);
                    _gridLines.Append(gridVertex3);
                    _gridLines.Append(gridVertex4);
                    _gridLines.Append(gridVertex4);
                    _gridLines.Append(gridVertex5);
                    _gridLines.Append(gridVertex5);
                    _gridLines.Append(gridVertex6);
                    _gridLines.Append(gridVertex6);
                    _gridLines.Append(gridVertex1);
                    #endregion
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

