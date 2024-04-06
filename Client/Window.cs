using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Runtime.InteropServices;
using VideoMode = SFML.Window.VideoMode;

namespace Client
{
    public class Window
    {
        private RenderWindow window;
        private Clock deltaClock;

        private Screen screen;

        public static void Main(String[] args)
        {
            Window window = new Window();
        }

        public Window()
        {
            window = new RenderWindow(VideoMode.DesktopMode, "Cettlers of Satan", Styles.Default, new ContextSettings() { AntialiasingLevel = 8 });

            // Maximize window
            ShowWindow(window.SystemHandle, 3);

            // Init Screen
            screen = new GameScreen(window);

            // Init ImGui
            GuiImpl.Init(window);

            window.KeyPressed += Window_KeyPressed;
            window.Closed += Window_Closed;
            window.KeyPressed += Window_KeyPressed;

            deltaClock = new Clock();

            while (window.IsOpen)
            {
                Time deltaTime = deltaClock.Restart();

                // Input handling
                if (window.HasFocus())
                {
                    screen.HandleInput(deltaTime);
                }

                // Update
                screen.Update(deltaTime);

                // Rendering
                screen.Draw(deltaTime);
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            window.Close();
        }

        private void Window_KeyPressed(object? sender, KeyEventArgs e)
        {
            if (sender != window) return;
            if (e.Code == Keyboard.Key.Escape) window.Close();
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
