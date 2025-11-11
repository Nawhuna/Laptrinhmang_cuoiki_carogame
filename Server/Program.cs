using System.Net;
using Server.Network;
using Server.Game;

namespace Server
{
    internal class Program
    {
        private static string _ipAddress = "127.0.0.1";
        private static int _port = 9000;
        private static GameManager _gameManager = new GameManager();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($" Server Caro khởi động tại {_ipAddress}:{_port}");

            var server = new ServerHost(_ipAddress, _port, _gameManager);
            await server.StartAsync();
        }
    }
}
