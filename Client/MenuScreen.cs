using Common;
using ImGuiNET;
using Microsoft.VisualBasic;
using SFML.Graphics;
using SFML.System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    public class MenuScreen : Screen
    {
        private RenderWindow _window;
        private string _username = "";
        private Vector3 _preferredColor = new Vector3(0.0f, 0.8f, 0.0f);
        private string? _error;

        private TcpClient _client;
        private Thread _recvThread;

        private string _ip = "127.0.0.1";
        private int _port = 17017;

        private object _lock = new object();
        private string _log = "";
        private string _msg = "";

        public MenuScreen(RenderWindow window)
        {
            _window = window;
            _client = new TcpClient();
        }

        public void Draw(Time deltaTime)
        {
            _window.Clear(new Color(8, 25, 75));

            ImGui.Begin("Account Settings", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.InputText("Username", ref _username, 20, ImGuiInputTextFlags.CharsNoBlank);
            ImGui.ColorEdit3("Preferred Color", ref _preferredColor);

            ImGui.Separator();

            ImGui.InputText("IP", ref _ip, 20, ImGuiInputTextFlags.CharsNoBlank);
            ImGui.InputInt("Port", ref _port);
            
            if (ImGui.Button("Join Server"))
            {
                try
                {
                    _error = null;
                    JoinServer();
                }
                catch (Exception e)
                {
                    _error = e.Message;
                }
            }

            if (ImGui.Button("Disconnect") && _client.Connected)
            {
                _client.Client.Shutdown(SocketShutdown.Send);
                //_recvThread.Interrupt();
                _client.Close();
                lock(_lock) _log += "Disconnected" + Environment.NewLine;
            }

            ImGui.Separator();

            lock(_lock) ImGui.InputTextMultiline("Log", ref _log, 524288, new Vector2(200, 150), ImGuiInputTextFlags.ReadOnly);
            
            if(ImGui.InputText("Input", ref _msg, 500, !_client.Connected ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.EnterReturnsTrue))
            {
                lock(_lock) _log += $"<Me> {_msg}" + Environment.NewLine;

                NetworkStream clientStream = _client.GetStream();
                byte[] sendBuffer = Encoding.UTF8.GetBytes(_msg);
                clientStream.Write(sendBuffer, 0, sendBuffer.Length);

                _msg = "";
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

        public void JoinServer()
        {
            if (_recvThread != null && _recvThread.IsAlive) throw new InvalidOperationException("Already connected to server");

            IPAddress ip = IPAddress.Parse(_ip);
            // New client instance required, since every client can only connect/disconnect once
            _client = new TcpClient();
            _client.Connect(ip, _port);

            lock (_lock) _log += $"Connected to server" + Environment.NewLine;
            
            // Spawn receive thread
            _recvThread = new Thread(o => Receive(_client, ref _log, ref _lock));
            _recvThread.Start();
        }

        static void Receive(TcpClient client, ref string log, ref object lockObj)
        {
            try
            {
                NetworkStream clientStream = client.GetStream();

                byte[] recvBuffer = new byte[1024];
                int recvCount;

                // Receive until zero-length reception
                while ((recvCount = clientStream.Read(recvBuffer, 0, recvBuffer.Length)) > 0)
                {
                    lock (lockObj) log += Encoding.UTF8.GetString(recvBuffer, 0, recvCount) + Environment.NewLine;
                }
            }
            catch(IOException e)
            {
                Console.WriteLine("Receive thread interrupted");
            }
        }
    }
}
