﻿using ImGuiNET;
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
        private View view;
        private Clock deltaClock;

        private Screen screen;

        public Window()
        {
            window = new RenderWindow(VideoMode.DesktopMode, "Cettlers of Satan", Styles.Default, new ContextSettings() { AntialiasingLevel = 8 });
            ShowWindow(window.SystemHandle, 3);
            view = new View(new Vector2f(0, 0), new Vector2f(window.Size.X, window.Size.Y));
            window.SetView(view);

            screen = new GameScreen(window, view);

            GuiImpl.Init(window);

            window.KeyPressed += Window_KeyPressed;
            window.Closed += Window_Closed;
            window.Resized += Window_Resized;
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

        private void Window_Resized(object? sender, SizeEventArgs e)
        {
            view.Size = new Vector2f(e.Width, e.Height);
            window.SetView(view);
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
