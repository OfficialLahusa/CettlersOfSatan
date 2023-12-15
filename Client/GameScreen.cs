using Common;
using ImGuiNET;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class GameScreen : Screen
    {
        private RenderWindow window;
        private View view;

        private HexMap<Tile> _map;
        private HexMapRenderer _renderer;

        public static Font Font;
        public static Texture Atlas;

        private float viewZoom = 1f;
        private float viewZoomBase = 1f;
        private const float moveSpeed = 300f;

        static GameScreen()
        {

            Font = new Font(@"res\Open_Sans\static\OpenSans-Regular.ttf");
            Atlas = new Texture(@"res\atlas.png");
        }

        public GameScreen(RenderWindow window, View view)
        {
            this.window = window;
            this.view = view;

            _map = MapGenerator.GenerateRandomClassic();

            Func<Tile, Color> tileColorFunc = val => val.Type switch
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
            Func<Tile, Color?> gridColorFunc = val => val.Type switch
            {
                TileType.Water or TileType.NonPlayable => null, // transparent for water/non-playable
                _ => new Color(0xd5, 0xbe, 0x84) // white for land tiles
            };

            _renderer = new HexMapRenderer(_map, tileColorFunc, gridColorFunc, 150, 25);

            this.window.MouseWheelScrolled += Window_MouseWheelScrolled;
        }

        public void Draw(Time deltaTime)
        {
            window.Clear(new Color(8, 25, 75));

            window.Draw(_renderer);

            ImGui.Begin("Menu", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Text($"FPS: {Math.Floor(1 / deltaTime.AsSeconds())}");
            ImGui.Text($"Frametime: {Math.Round(deltaTime.AsMicroseconds() / 1000.0f, 3)}ms");

            if (ImGui.Button("Generate"))
            {
                _map = MapGenerator.GenerateRandomClassic();
                _renderer.Map = _map;
                _renderer.Update();
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
