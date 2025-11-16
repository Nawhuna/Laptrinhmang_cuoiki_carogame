using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.Game;

namespace Server.Network
{
    public class ClientHandler
    {
        private readonly Socket _socket;
        private readonly ServerHost _host;
        private readonly GameManager _gm;
        private readonly StringBuilder _buf = new();
        private GameRoom? _room;
        private ClientRef _self;

        public ClientHandler(Socket s, ServerHost host, GameManager gm)
        {
            _socket = s; _host = host; _gm = gm;
            _self = new ClientRef();
        }

        public async Task StartAsync()
        {
            try
            {
                byte[] buffer = new byte[2048];
                while (_socket.Connected)
                {
                    int n = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (n == 0) break;

                    var chunk = Encoding.UTF8.GetString(buffer, 0, n);
                    _buf.Append(chunk);

                    while (true)
                    {
                        var cur = _buf.ToString();
                        int idx = cur.IndexOf('\n');
                        if (idx < 0) break;

                        var line = cur[..idx].Trim();
                        _buf.Remove(0, idx + 1);
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        Process(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Client error: {ex.Message}");
            }
            finally
            {
                if (_room != null)
                {
                    _room.Leave(_self.PlayerId);
                    _gm.RemoveRoom(_room);
                }
                _host.Remove(this);
                try { _socket.Close(); } catch { }
            }
        }

        private void Process(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string action = doc.RootElement.GetProperty("Action").GetString() ?? "";

            switch (action)
            {
                case "JOIN":
                    _self.Name = doc.RootElement.TryGetProperty("Name", out var nProp)
                        ? nProp.GetString() ?? "Player"
                        : "Player";
                    _self.SendJsonLine = SendJsonLine;
                    _self.PlayerId = Guid.NewGuid().ToString();
                    _room = _gm.AssignRoom(_self);
                    break;

                case "MOVE":
                    if (_room == null) return;
                    int x = doc.RootElement.GetProperty("X").GetInt32();
                    int y = doc.RootElement.GetProperty("Y").GetInt32();
                    _room.HandleMove(_self.PlayerId, x, y);
                    break;

                case "CHAT":
                    {
                        string pid = root.GetProperty("PlayerId").GetString() ?? "";
                        string msg = root.GetProperty("Message").GetString() ?? "";

                        _room?.BroadcastChat(pid, msg);
                    }
                    break;

                case "SURRENDER":
                    {
                        string pid = root.GetProperty("PlayerId").GetString() ?? "";
                        _room?.HandleSurrender(pid);
                        break;
                    }
            }
        }

        private void SendJsonLine(string json)
        {
            string line = json + "\n";
            byte[] data = Encoding.UTF8.GetBytes(line);
            try { _socket.Send(data); } catch { }
        }
    }
}
