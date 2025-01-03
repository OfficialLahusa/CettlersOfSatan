﻿using Common;
using ImGuiNET;
using Microsoft.VisualBasic;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Edge = Common.Edge;

namespace Client
{
    public class GameScreen : Screen
    {
        private RenderWindow _window;
        private View _mapView;
        private View _uiView;

        private GameState _state;

        private BoardRenderer _renderer;
        private CardWidget _cardWidget;
        private DiceWidget _diceWidget;

        private EventLog _eventLog;
        private float[] _rollDistribution;

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

        // Point-and-click building
        private CircleShape _intersectionHitbox;
        private RectangleShape _edgeHitbox;
        private CircleShape _centerHitbox;
        private Sound _placeSound;

        static GameScreen()
        {
            Font = new Font(@"..\..\..\res\Open_Sans\static\OpenSans-Regular.ttf");
        }

        public GameScreen(RenderWindow window)
        {
            _window = window;

            _state = new GameState(MapGenerator.GenerateRandomClassic(), PLAYER_COUNT);

            _renderer = new BoardRenderer(_state.Board, 120, 20);
            _cardWidget = new CardWidget(window, _state.Players[_playerIndex].CardSet);
            _diceWidget = new DiceWidget(window);
            _diceWidget.Active = true;

            _eventLog = new EventLog();

            // Bucket indices are the roll totals
            // Padded for symmetric plotting with correct indices
            _rollDistribution = new float[15];

            _intersectionHitbox = new CircleShape(_renderer.SideLength / 4, 32);
            _intersectionHitbox.Origin = new Vector2f(_intersectionHitbox.Radius, _intersectionHitbox.Radius);

            _edgeHitbox = new RectangleShape(new Vector2f(_renderer.SideLength * 0.35f, _renderer.SideLength));
            _edgeHitbox.Origin = _edgeHitbox.Size / 2;

            _centerHitbox = new CircleShape(_renderer.SideLength * 0.85f, 32);
            _centerHitbox.Origin = new Vector2f(_centerHitbox.Radius, _centerHitbox.Radius);

            _placeSound = new Sound(Sounds.Place);
            _placeSound.Volume = 40f;

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
            ImGui.PushStyleColor(ImGuiCol.Text, ColorPalette.ColorToVec4(playerColor));
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

                _cardWidget.SetCardSet(_state.Players[_playerIndex].CardSet);

                _eventLog.WriteLine(new SeparatorEntry());
                _eventLog.WriteLine(new StrEntry("Switching to"), new PlayerEntry(_playerIndex));
            }
            ImGui.PopStyleColor();

            ImGui.Separator();

            ImGui.Checkbox("Show Yield Points", ref _renderer.DrawYieldPoints);
            ImGui.Checkbox("Show Shadows", ref _renderer.DrawTokenShadows);
            ImGui.Checkbox("Show Intersections", ref _renderer.DrawIntersectionMarkers);
            ImGui.Checkbox("Show Edges", ref _renderer.DrawEdgeMarkers);
            ImGui.Checkbox("Show Centers", ref _renderer.DrawCenterMarkers);

            ImGui.Separator();

            ImGui.Checkbox("Center Desert", ref _centerDesert);

            if (ImGui.Button("Generate [Space]"))
            {
                RegenerateMap();
            }

            ImGui.Separator();

            ImGui.TextUnformatted("Shop:");

            DisplayShop();

            ImGui.Separator();
            
            _eventLog.Draw();

            ImGui.Separator();

            if(ImGui.TreeNode("Analytics"))
            {
                ImGui.TextUnformatted("Roll Result Distibution:");
                ImGui.PlotHistogram(string.Empty, ref _rollDistribution[0], _rollDistribution.Length, 0, string.Empty, 0, _rollDistribution.Max(), new System.Numerics.Vector2(225, 100));
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
            _state.Board = MapGenerator.GenerateRandomClassic(_centerDesert);
            _state.ResetCards();

            _eventLog.Clear();

            _renderer.Board = _state.Board;
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
                RollDice();
            }
        }

        private void Window_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
        {
            // Mouse pos in UI view
            Vector2f uiMousePos = _window.MapPixelToCoords(new Vector2i(e.X, e.Y), _uiView);

            // Dice widget clicking
            if (e.Button == Mouse.Button.Left && _diceWidget.Contains(uiMousePos.X, uiMousePos.Y))
            {
                RollDice();
                return;
            }

            // Mouse pos in map view
            Vector2f mapMousePos = _window.MapPixelToCoords(new Vector2i(e.X, e.Y), _mapView);

            // Intersection clicking
            foreach (Intersection intersection in _state.Board.Intersections)
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

                        if (intersection.Building == Intersection.BuildingType.Settlement)
                        {
                            _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("placed a settlement"));
                        }
                        else if(intersection.Building == Intersection.BuildingType.City)
                        {
                            _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("promoted a settlement to a city"));
                        }
                    }

                    // Claim intersection for current player
                    intersection.Owner = _playerIndex;

                    // Play place sound
                    _placeSound.Play();

                    // Rebuild geometry
                    _renderer.Update();

                    return;
                }
            }

            // Edge clicking
            foreach (Edge edge in _state.Board.Edges)
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

                        if(edge.Building == Edge.BuildingType.Road)
                        {
                            _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("placed a road"));
                        }
                    }

                    // Claim edge for current player
                    edge.Owner = _playerIndex;

                    // Play place sound
                    _placeSound.Play();

                    // Rebuild geometry
                    _renderer.Update();

                    return;
                }
            }

            // Center clicking
            foreach(Tile tile in _state.Board.Map.Where(x => x.IsLandTile()))
            {
                _centerHitbox.Position = _renderer.GetTileCenter(tile);

                float dist = MathF.Sqrt(
                    MathF.Pow(mapMousePos.X - _centerHitbox.Position.X, 2)
                    + MathF.Pow(mapMousePos.Y - _centerHitbox.Position.Y, 2)
                );

                // Click was on intersection
                if (e.Button == Mouse.Button.Left && dist < _centerHitbox.Radius)
                {
                    // Move robber
                    _state.Board.Robber = tile;

                    // Play place sound
                    _placeSound.Play();

                    _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("moved the robber"));

                    return;
                }
            }
        }

        public void DisplayShop()
        {
            CardSet playerHand = _state.Players[_playerIndex].CardSet;

            bool canBuildRoad = _state.CanBuildRoad(_playerIndex);
            bool canBuildSettlement = _state.CanBuildSettlement(_playerIndex);
            bool canBuildCity = _state.CanBuildCity(_playerIndex);
            bool canBuyDevelopmentCard = _state.CanBuyDevelopmentCard(_playerIndex);

            if (!canBuildRoad)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }
            if (ImGui.Button("Build Road") && canBuildRoad)
            {
                _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("built a road"));

                playerHand.Remove(CardSet.CardType.Lumber, 1);
                playerHand.Remove(CardSet.CardType.Brick, 1);
            }
            if (!canBuildRoad)
            {
                ImGui.PopStyleVar();
            }

            if (!canBuildSettlement)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }
            if (ImGui.Button("Build Settlement") && canBuildSettlement)
            {
                _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("built a settlement"));

                playerHand.Remove(CardSet.CardType.Lumber, 1);
                playerHand.Remove(CardSet.CardType.Brick, 1);
                playerHand.Remove(CardSet.CardType.Wool, 1);
                playerHand.Remove(CardSet.CardType.Grain, 1);
            }
            if (!canBuildSettlement)
            {
                ImGui.PopStyleVar();
            }


            if (!canBuildCity)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }
            if (ImGui.Button("Build City") && canBuildCity)
            {
                _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry("built a city"));

                playerHand.Remove(CardSet.CardType.Grain, 2);
                playerHand.Remove(CardSet.CardType.Ore, 3);
            }
            if (!canBuildCity)
            {
                ImGui.PopStyleVar();
            }

            if (!canBuyDevelopmentCard)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }
            if (ImGui.Button("Buy Development Card") && canBuyDevelopmentCard)
            {
                // Draw random development card from bank (might be empty)
                CardSet.CardType? drawnType = _state.Bank.DrawByType(CardSet.DEVELOPMENT_CARD_TYPES);

                if (drawnType.HasValue)
                {
                    playerHand.Add(drawnType.Value, 1);

                    playerHand.Remove(CardSet.CardType.Wool, 1);
                    playerHand.Remove(CardSet.CardType.Grain, 1);
                    playerHand.Remove(CardSet.CardType.Ore, 1);
                }
            }
            if (!canBuyDevelopmentCard)
            {
                ImGui.PopStyleVar();
            }
        }

        public void RollDice()
        {
            int total = _diceWidget.Roll();

            _rollDistribution[total] += 1;

            _eventLog.WriteLine(new SeparatorEntry());
            _eventLog.WriteLine(new PlayerEntry(_playerIndex), new StrEntry($"rolled {_diceWidget.Total} ({_diceWidget.First}+{_diceWidget.Second})"));

            if (total == 7)
            {
                // TODO: Trigger Robber
                _eventLog.WriteLine(new StrEntry("The robber was triggered"));
            }
            else
            {
                // Award yields
                (uint[,] yieldSummary, uint robbedYields) = _state.AwardYields(total);

                for (int player = 0; player < yieldSummary.GetLength(0); player++)
                {
                    for (int resource = 0; resource < yieldSummary.GetLength(1); resource++)
                    {
                        uint yieldAmount = yieldSummary[player, resource];

                        if (yieldAmount > 0)
                        {
                            _eventLog.WriteLine(
                                new PlayerEntry(player),
                                new StrEntry($"earned {yieldAmount}"),
                                new CardEntry(CardSet.RESOURCE_CARD_TYPES[resource])
                            );
                        }
                    }
                }

                if(robbedYields > 0)
                {
                    string msg = $"The robber stole {robbedYields} yield";

                    if (robbedYields > 1) msg += "s";

                    _eventLog.WriteLine(new StrEntry(msg));
                }
            }
        }
    }
}
