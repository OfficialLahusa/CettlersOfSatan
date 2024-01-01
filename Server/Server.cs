using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Server
    {
        // Lock for thread-safe usage of client dictionary
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> _clients = new Dictionary<int, TcpClient>();
        const int port = 17017;

        // Main server thread
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Cettlers of Satan Server");

            // Index of next client
            int clientIndex = 0;

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine($"Server listening on Port {port}");
            
            // Listen for incoming client connections indefinitely
            while(true)
            {
                TcpClient client = listener.AcceptTcpClient();

                // Thread-safely add client to dictionary
                lock (_lock) _clients.Add(clientIndex, client);
                Console.WriteLine($"{client.Client.RemoteEndPoint} connected");

                // Spawn new client thread
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(clientIndex++);
            }
        }

        // Client handler thread delegate (one per client)
        public static void HandleClient(object? idx)
        {
            if(idx == null) throw new ArgumentNullException("Client index was null");
            int clientIndex = (int)idx;

            // Thread-safely get client from dictionary
            TcpClient client;
            lock(_lock) client = _clients[clientIndex];

            // Listen indefinitely
            while(true)
            {
                NetworkStream clientStream = client.GetStream();

                byte[] recvBuffer = new byte[1024];
                int recvCount = clientStream.Read(recvBuffer, 0, recvBuffer.Length);

                // Quit when 0 bytes are received
                if (recvCount == 0) break;

                string msg = Encoding.UTF8.GetString(recvBuffer, 0, recvCount);
                Broadcast($"<{idx}> " + msg, clientIndex);
                Console.WriteLine($"<{idx}> " + msg);
            }

            // Remove client from dictionary upon termination
            lock(_lock) _clients.Remove(clientIndex);
        }

        public static void Broadcast(string msg, int? sourceIndex = null)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(msg);

            // Thread-safely iterate over clients for bcast
            lock (_lock)
            {
                foreach ((int index, TcpClient client) in _clients)
                {
                    // Do not send back to source
                    if (sourceIndex.HasValue && index == sourceIndex.Value) continue;

                    client.GetStream().Write(sendBuffer, 0, sendBuffer.Length);
                }
            }
        }
    }
}