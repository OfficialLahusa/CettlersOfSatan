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
        private string _username = "";
        private Vector3 _preferredColor = new Vector3(0.0f, 0.8f, 0.0f);
        private string? _error;

        public MenuScreen(RenderWindow window)
        {
            _window = window;
        }

        public void Draw(Time deltaTime)
        {
            _window.Clear(new Color(8, 25, 75));

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
