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
        private RenderWindow _window;
        private View _mapView;
        private View _uiView;

        private Board _board;
        private BoardRenderer _renderer;
        private DiceWidget _diceWidget;

        public static Font Font;

        // View movement
        private float _viewZoom = 1f;
        private float _viewZoomBase = 1f;
        private const float _moveSpeed = 300f;

        // FPS Counter
        private float _frameTimeSum = 0.0f;
        private int _frameTimeCount = 0;
        private float _latestAvgFrameTime = 0.0f;

        static GameScreen()
        {
            Font = new Font(@"..\..\..\res\Open_Sans\static\OpenSans-Regular.ttf");
        }

        public GameScreen(RenderWindow window)
        {
            _window = window;

            _board = MapGenerator.GenerateRandomClassic();
            _renderer = new BoardRenderer(_board, 120, 20);
            _diceWidget = new DiceWidget(window);

            _mapView = new View(ClientUtils.RoundVec2f(_renderer.GetTileCenter(3, 3)), new Vector2f(window.Size.X, window.Size.Y)); ;
            _uiView = new View(new Vector2f(0, 0), _mapView.Size);

            _window.SetView(_mapView);

            _window.MouseWheelScrolled += Window_MouseWheelScrolled;
            _window.Resized += Window_Resized;
            _window.KeyPressed += Window_KeyPressed;
            _window.MouseButtonPressed += Window_MouseButtonPressed;
        }

        public void Draw(Time deltaTime)
        {
            _window.Clear(new Color(8, 25, 75));

            // Draw map
            _window.SetView(_mapView);

            _window.Draw(_renderer);

            // Draw UI
            _window.SetView(_uiView);

            _window.Draw(_diceWidget);

            ImGui.Begin("Menu", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Text($"FPS: {Math.Floor(1 / _latestAvgFrameTime)}");
            ImGui.Text($"Frametime: {_latestAvgFrameTime * 1000.0f:0.000}ms");

            ImGui.Separator();

            ImGui.Checkbox("Show Shadows", ref _renderer.DrawTokenShadows);
            ImGui.Checkbox("Show Intersections", ref _renderer.DrawIntersectionMarkers);

            if (ImGui.Button("Generate [Space]"))
            {
                RegenerateMap();
            }

            ImGui.End();
            GuiImpl.Render(_window);

            _window.Display();
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
            moveDelta *= deltaTime.AsSeconds() * _moveSpeed * _viewZoom;
            _mapView.Move(moveDelta);

            if(Keyboard.IsKeyPressed(Keyboard.Key.Space))
            {
                RegenerateMap();
            }
        }

        public void Update(Time deltaTime)
        {
            // Update FPS counter every 0.5s. Is initialized on the first frame
            _frameTimeCount++;
            _frameTimeSum += deltaTime.AsSeconds();
            if (_frameTimeSum > 0.5f || _latestAvgFrameTime <= 0.000001f)
            {
                _latestAvgFrameTime = _frameTimeSum / _frameTimeCount;
                _frameTimeSum = 0.0f;
                _frameTimeCount = 0;
            }

            _window.DispatchEvents();
            GuiImpl.Update(_window, deltaTime);
        }

        private void RegenerateMap()
        {
            _board = MapGenerator.GenerateRandomClassic();
            _renderer.Board = _board;
            _renderer.Update();
        }

        private void Window_MouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
        {
            _mapView.Zoom(1 / _viewZoom);
            _viewZoomBase = (float)Math.Max(0.001, _viewZoomBase - e.Delta);
            _viewZoom = (float)Math.Pow(1.3, _viewZoomBase) / 1.3f;
            _mapView.Zoom(_viewZoom);

            _mapView.Size = ClientUtils.RoundVec2f(_mapView.Size);
        }

        private void Window_Resized(object? sender, SizeEventArgs e)
        {
            _mapView.Size = new Vector2f(e.Width, e.Height);
            _uiView.Size = new Vector2f(e.Width, e.Height);
        }

        private void Window_KeyPressed(object? sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Enter)
            {
                _diceWidget.Roll();
            }
        }

        private void Window_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
        {
            Vector2f mousePos = _window.MapPixelToCoords(new Vector2i(e.X, e.Y), _uiView);
            if (e.Button == Mouse.Button.Left && _diceWidget.Contains(mousePos.X, mousePos.Y))
            {
                _diceWidget.Roll();
            }
        }
    }
}
