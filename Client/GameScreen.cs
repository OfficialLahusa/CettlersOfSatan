using ImGuiNET;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class GameScreen : Screen
    {
        private RenderWindow window;
        private View view;

        private float viewZoom = 1f;
        private float viewZoomBase = 1f;
        private const float moveSpeed = 300f;

        private int snappingFunctionIdx = 0;
        private String[] snappingFunctions =
        {
            "None",
            "Centers",
            "Corners"
        };

        private VertexArray grid;
        private System.Numerics.Vector3 lineColorVec;
        private System.Numerics.Vector3 backgroundColorVec;
        private int gridWidth = 10;
        private int gridHeight = 10;
        private const float gridCellWidth = 100f;
        private const float gridBorderWidth = .1f * gridCellWidth;
        private const float gridBorderHalfWidth = gridBorderWidth / 2f;

        public GameScreen(RenderWindow window, View view)
        {
            this.window = window;
            this.view = view;

            grid = new VertexArray(PrimitiveType.Triangles);
            lineColorVec = new System.Numerics.Vector3(.1f, .1f, .1f);
            backgroundColorVec = new System.Numerics.Vector3(.9f, .9f, .9f);

            this.window.MouseWheelScrolled += Window_MouseWheelScrolled;

            GenerateGrid(ref grid, Util.Vec3ToColor(lineColorVec), Util.Vec3ToColor(backgroundColorVec));
        }

        public void Draw(Time deltaTime)
        {
            window.Clear(new Color(8, 25, 75));

            window.Draw(grid);

            //ImGui.ShowDemoWindow();
            ImGui.Begin("Grid Utilities", ImGuiWindowFlags.AlwaysAutoResize);

            if (ImGui.BeginCombo("Snapping Function", snappingFunctions[snappingFunctionIdx]))
            {
                for (int i = 0; i < snappingFunctions.Length; i++)
                {
                    bool selected = snappingFunctionIdx == i;
                    if (ImGui.Selectable(snappingFunctions[i], selected))
                    {
                        snappingFunctionIdx = i;
                    }
                    if (selected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.Separator();

            ImGui.SliderInt("Width", ref gridWidth, 1, 100);
            ImGui.SliderInt("Height", ref gridHeight, 1, 100);
            ImGui.ColorEdit3("Lines", ref lineColorVec);
            ImGui.ColorEdit3("Background", ref backgroundColorVec);
            ImGui.Text("Current Vertices: " + grid.VertexCount.ToString());
            if (ImGui.Button("Generate"))
            {
                GenerateGrid(ref grid, Util.Vec3ToColor(lineColorVec), Util.Vec3ToColor(backgroundColorVec));
            }

            ImGui.End();
            GuiImpl.Render(window);

            window.Display();
        }

        public void HandleInput(Time deltaTime)
        {
            Vector2f moveDelta = new Vector2f();
            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                moveDelta.Y -= 1;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                moveDelta.Y += 1;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                moveDelta.X -= 1;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                moveDelta.X += 1;
            }
            moveDelta *= deltaTime.AsSeconds() * moveSpeed * viewZoom;
            view.Move(moveDelta);
            window.SetView(view);
        }

        public void Update(Time deltaTime)
        {
            window.DispatchEvents();
            GuiImpl.Update(window, deltaTime);
        }

        private void GenerateGrid(ref VertexArray grid, Color lineColor, Color backgroundColor)
        {
            grid.Clear();

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // Outer points
                    Vector2f topLeft = new Vector2f(gridBorderHalfWidth + x * gridCellWidth, gridBorderHalfWidth + y * gridCellWidth);
                    Vector2f topRight = new Vector2f(gridBorderHalfWidth + x * gridCellWidth + gridCellWidth, gridBorderHalfWidth + y * gridCellWidth);
                    Vector2f bottomLeft = new Vector2f(gridBorderHalfWidth + x * gridCellWidth, gridBorderHalfWidth + y * gridCellWidth + gridCellWidth);
                    Vector2f bottomRight = new Vector2f(gridBorderHalfWidth + x * gridCellWidth + gridCellWidth, gridBorderHalfWidth + y * gridCellWidth + gridCellWidth);

                    // Inset points for given grid border width
                    Vector2f innerTopLeft = topLeft + new Vector2f(gridBorderHalfWidth, gridBorderHalfWidth);
                    Vector2f innerTopRight = topRight + new Vector2f(-gridBorderHalfWidth, gridBorderHalfWidth);
                    Vector2f innerBottomLeft = bottomLeft + new Vector2f(gridBorderHalfWidth, -gridBorderHalfWidth);
                    Vector2f innerBottomRight = bottomRight + new Vector2f(-gridBorderHalfWidth, -gridBorderHalfWidth);

                    // Double outermost border thickness
                    topLeft += new Vector2f(x == 0 ? -gridBorderHalfWidth : 0, y == 0 ? -gridBorderHalfWidth : 0);
                    topRight += new Vector2f(x == (gridWidth - 1) ? gridBorderHalfWidth : 0, y == 0 ? -gridBorderHalfWidth : 0);
                    bottomRight += new Vector2f(x == (gridWidth - 1) ? gridBorderHalfWidth : 0, y == (gridHeight - 1) ? gridBorderHalfWidth : 0);
                    bottomLeft += new Vector2f(x == 0 ? -gridBorderHalfWidth : 0, y == (gridHeight - 1) ? gridBorderHalfWidth : 0);

                    // Grid border triangles
                    grid.Append(new Vertex(topLeft, lineColor));
                    grid.Append(new Vertex(topRight, lineColor));
                    grid.Append(new Vertex(bottomLeft, lineColor));

                    grid.Append(new Vertex(bottomLeft, lineColor));
                    grid.Append(new Vertex(bottomRight, lineColor));
                    grid.Append(new Vertex(topRight, lineColor));

                    // Grid background triangles
                    grid.Append(new Vertex(innerTopLeft, backgroundColor));
                    grid.Append(new Vertex(innerTopRight, backgroundColor));
                    grid.Append(new Vertex(innerBottomLeft, backgroundColor));

                    grid.Append(new Vertex(innerBottomLeft, backgroundColor));
                    grid.Append(new Vertex(innerBottomRight, backgroundColor));
                    grid.Append(new Vertex(innerTopRight, backgroundColor));
                }
            }
        }

        private void Window_MouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
        {
            view.Zoom(1 / viewZoom);
            viewZoomBase = (float)Math.Max(0.001, viewZoomBase - e.Delta);
            viewZoom = (float)Math.Pow(1.3, viewZoomBase) / 1.3f;
            view.Zoom(viewZoom);
            window.SetView(view);
        }
    }
}
