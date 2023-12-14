using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Cettlers of Satan Server");

            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[1];
            const int port = 17017;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Console.WriteLine($"Server listening on {ipAddress}:{port}");

            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(localEndPoint);
            socket.Listen(5);
            Socket handler = socket.Accept();
        }
    }
}