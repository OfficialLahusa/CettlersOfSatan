using Common;
using ImGuiNET;
using Microsoft.VisualBasic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static SFML.Graphics.Text;

namespace Client
{
    public class GameScreen : Screen
    {
        private RenderWindow _window;
        private View _mapView;
        private View _uiView;

        private Board _board;
        private CardSet _cardSet;

        private BoardRenderer _renderer;
        private CardWidget _cardWidget;
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

        // Map generator options
        private bool _centerDesert = false;

        // Gameplay options
        private int _playerIndex = 0;
        public const int PLAYER_COUNT = 4;

        // Click hitboxes
        private CircleShape _intersectionHitbox;
        private RectangleShape _edgeHitbox;

        static GameScreen()
        {
            Font = new Font(@"..\..\..\res\Open_Sans\static\OpenSans-Regular.ttf");
        }

        public GameScreen(RenderWindow window)
        {
            _window = window;

            _board = MapGenerator.GenerateRandomClassic();
            _cardSet = CardSet.CreateSample();

            _renderer = new BoardRenderer(_board, 120, 20);
            _cardWidget = new CardWidget(window, _cardSet);
            _diceWidget = new DiceWidget(window);
            _diceWidget.Active = true;

            _intersectionHitbox = new CircleShape(_renderer.SideLength / 4, 32);
            _intersectionHitbox.Origin = new Vector2f(_intersectionHitbox.Radius, _intersectionHitbox.Radius);

            _edgeHitbox = new RectangleShape(new Vector2f(_renderer.SideLength * 0.35f, _renderer.SideLength));
            _edgeHitbox.Origin = _edgeHitbox.Size / 2;

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

            _window.Draw(_cardWidget);
            _window.Draw(_diceWidget);

            ImGui.Begin("Menu", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Text($"FPS: {Math.Floor(1 / _latestAvgFrameTime)}");
            ImGui.Text($"Frametime: {_latestAvgFrameTime * 1000.0f:0.000}ms");

            ImGui.Separator();

            Color playerColor = ColorPalette.GetPlayerColor(_playerIndex);
            System.Numerics.Vector4 imPlayerColor = new System.Numerics.Vector4(playerColor.R, playerColor.G, playerColor.B, playerColor.A);
            ImGui.PushStyleColor(ImGuiCol.Text, imPlayerColor);
            if (ImGui.InputInt("Player", ref _playerIndex))
            {
                if (_playerIndex < 0)
                {
                    _playerIndex = PLAYER_COUNT - 1;
                }
                else if (_playerIndex > PLAYER_COUNT - 1)
                {
                    _playerIndex = 0;
                }
            }
            ImGui.PopStyleColor();

            ImGui.Separator();

            ImGui.Checkbox("Show Shadows", ref _renderer.DrawTokenShadows);
            ImGui.Checkbox("Show Intersections", ref _renderer.DrawIntersectionMarkers);
            ImGui.Checkbox("Show Edges", ref _renderer.DrawEdgeMarkers);

            ImGui.Separator();

            ImGui.Checkbox("Center Desert", ref _centerDesert);

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

            Vector2f uiMousePos = _window.MapPixelToCoords(Mouse.GetPosition(_window), _uiView);
            _cardWidget.Update(deltaTime.AsSeconds(), uiMousePos);

            _window.DispatchEvents();
            GuiImpl.Update(_window, deltaTime);
        }

        private void RegenerateMap()
        {
            _board = MapGenerator.GenerateRandomClassic(_centerDesert);
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
            // Mouse pos in UI view
            Vector2f uiMousePos = _window.MapPixelToCoords(new Vector2i(e.X, e.Y), _uiView);

            // Dice widget clicking
            if (e.Button == Mouse.Button.Left && _diceWidget.Contains(uiMousePos.X, uiMousePos.Y))
            {
                _diceWidget.Roll();
                return;
            }

            // Mouse pos in map view
            Vector2f mapMousePos = _window.MapPixelToCoords(new Vector2i(e.X, e.Y), _mapView);

            // Intersection clicking
            foreach (Intersection intersection in _board.Intersections)
            {
                _intersectionHitbox.Position = _renderer.GetIntersectionCenter(intersection);

                float dist = MathF.Sqrt(
                    MathF.Pow(mapMousePos.X - _intersectionHitbox.Position.X, 2)
                    + MathF.Pow(mapMousePos.Y - _intersectionHitbox.Position.Y, 2)
                );

                // Click was on intersection
                if (e.Button == Mouse.Button.Left && dist < _intersectionHitbox.Radius)
                {
                    // Clear previous buildings
                    if(intersection.Owner != _playerIndex && intersection.Building != Intersection.BuildingType.None)
                    {
                        intersection.Building = Intersection.BuildingType.None;
                    }
                    // Cycle own buildings
                    else
                    {
                        intersection.Building = intersection.Building switch
                        {
                            Intersection.BuildingType.None => Intersection.BuildingType.Settlement,
                            Intersection.BuildingType.Settlement => Intersection.BuildingType.City,
                            Intersection.BuildingType.City => Intersection.BuildingType.None,
                            _ => throw new InvalidOperationException()
                        };
                    }

                    // Claim intersection for current player
                    intersection.Owner = _playerIndex;

                    // Rebuild geometry
                    _renderer.Update();

                    return;
                }
            }

            // Edge clicking
            foreach (Edge edge in _board.Edges)
            {
                // Transform hitbox
                (Vector2f left, Vector2f right) = _renderer.GetEdgePoints(edge);

                _edgeHitbox.Position = (left + right) / 2f;

                _edgeHitbox.Rotation = edge.Direction switch
                {
                    Direction.Edge.UpDown => 0f,
                    Direction.Edge.LeftTop => -60f,
                    Direction.Edge.RightTop => 60f,
                    _ => throw new InvalidOperationException()
                };

                // Apply inverse transform to mouse pos for local AABB alignment
                Vector2f transformedMousePos = _edgeHitbox.InverseTransform.TransformPoint(mapMousePos);

                // Click was on edge
                if (e.Button == Mouse.Button.Left && _edgeHitbox.GetLocalBounds().Contains(transformedMousePos.X, transformedMousePos.Y))
                {
                    // Clear previous roads
                    if(edge.Owner != _playerIndex && edge.Building == Edge.BuildingType.Road)
                    {
                        edge.Building = Edge.BuildingType.None;
                    }
                    // Toggle own roads
                    else
                    {
                        edge.Building = edge.Building switch
                        {
                            Edge.BuildingType.None => Edge.BuildingType.Road,
                            Edge.BuildingType.Road => Edge.BuildingType.None,
                            _ => throw new InvalidOperationException(),
                        };
                    }

                    // Claim edge for current player
                    edge.Owner = _playerIndex;

                    // Rebuild geometry
                    _renderer.Update();

                    return;
                }
            }
        }
    }
}
