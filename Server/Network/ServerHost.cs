using System.Net;
using System.Net.Sockets;
using Server.Game;

namespace Server.Network
{
    public class ServerHost
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly GameManager _gm;
        private readonly List<ClientHandler> _clients = new();

        public ServerHost(string ip, int port, GameManager gm)
        {
            _ip = ip; _port = port; _gm = gm;
        }

        public async Task StartAsync()
        {
            var ep = new IPEndPoint(IPAddress.Parse(_ip), _port);
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ep);
            listener.Listen(100);

            Console.WriteLine($" Server listening on {_ip}:{_port}");

            while (true)
            {
                var s = await listener.AcceptAsync();
                var ch = new ClientHandler(s, this, _gm);
                lock (_clients) _clients.Add(ch);
                _ = ch.StartAsync();
            }
        }

        internal void Remove(ClientHandler c)
        {
            lock (_clients) _clients.Remove(c);
        }
    }
}
