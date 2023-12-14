using Common;
using ImGuiNET;
using SFML.Graphics;
using SFML.System;
using System.Numerics;

namespace Client
{
    public class MenuScreen : Screen
    {
        private RenderWindow _window;
        private HexMap<int> _map;
        private HexMapRenderer<int> _renderer;
        private string _username = "";
        private Vector3 _preferredColor = new Vector3(0.0f, 0.8f, 0.0f);
        private string? _error;

        public MenuScreen(RenderWindow window)
        {
            _window = window;
            _map = new HexMap<int>(10, 10, 0);
            
            Random random = new Random();

            for(int y = 0; y < _map.Height; y++)
            {
                for(int x = 0; x < _map.Width; x++)
                {
                    _map.SetTile(x, y, Convert.ToInt32(random.Next(3) == 0));
                }
            }

            _renderer = new HexMapRenderer<int>(_map, val => (val > 0) ? Color.Green : Color.Black, 50);
        }

        public void Draw(Time deltaTime)
        {
            _window.Clear(new Color(8, 25, 75));

            _window.Draw(_renderer);

            ImGui.Begin("Account Settings", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.InputText("Username", ref _username, 20, ImGuiInputTextFlags.CharsNoBlank);
            ImGui.ColorEdit3("Preferred Color", ref _preferredColor);
            
            if (ImGui.Button("Join Game"))
            {
                Console.WriteLine("Joining game");
                _error = "Can't connect to server";
            }

            if (_error != null)
            {
                ImGui.TextColored(new Vector4(0.65f, 0.0f, 0.0f, 1.0f), _error);
            }

            ImGui.End();
            GuiImpl.Render(_window);

            _window.Display();
        }

        public void HandleInput(Time deltaTime)
        {

        }

        public void Update(Time deltaTime)
        {
            _window.DispatchEvents();
            GuiImpl.Update(_window, deltaTime);
        }
    }
}
