using ImGuiNET;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class MenuScreen : Screen
    {
        private RenderWindow window;
        private string username = "";
        private Vector3 preferredColor = new Vector3(0.0f, 0.8f, 0.0f);
        private string? error;

        public MenuScreen(RenderWindow window)
        {
            this.window = window;
        }

        public void Draw(Time deltaTime)
        {
            window.Clear(new Color(8, 25, 75));

            //ImGui.ShowDemoWindow();
            ImGui.Begin("Account Settings", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.InputText("Username", ref username, 20, ImGuiInputTextFlags.CharsNoBlank);
            ImGui.ColorEdit3("Preferred Color", ref preferredColor);
            
            if (ImGui.Button("Join Game"))
            {
                Console.WriteLine("Joining game");
                error = "Can't connect to server";
            }

            if (error != null)
            {
                ImGui.TextColored(new Vector4(0.65f, 0.0f, 0.0f, 1.0f), error);
            }

            ImGui.End();
            GuiImpl.Render(window);

            window.Display();
        }

        public void HandleInput(Time deltaTime)
        {

        }

        public void Update(Time deltaTime)
        {
            window.DispatchEvents();
            GuiImpl.Update(window, deltaTime);
        }
    }
}
