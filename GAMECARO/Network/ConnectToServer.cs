using Client.Game;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Client.Network
{
    public class LeaderboardEntry
    {
        public string PlayerId { get; set; } = "";
        public int Score { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }

    public class ConnectToServer
    {
        private readonly string _ip;
        private readonly int _port;
        private Socket? _client;
        private CancellationTokenSource? _cts;
        private readonly StringBuilder _buf = new();
        private readonly Board _board;

        public string PlayerId { get; private set; } = "";
        public string Mark { get; private set; } = "?";

        public event Action? OnBoardChanged;
        public event Action<string>? OnWinner;
        public event Action<string>? OnInitReceived;
        public event Action<int, string>? OnTimerUpdate;
        public event Action? OnReset;
        public event Action<string, string>? OnChatReceived;


        // thêm event rank
        public event Action<int, int, int>? OnRankUpdate;
        
        // thêm event leaderboard
        public event Action<List<LeaderboardEntry>>? OnLeaderboardReceived;

        public ConnectToServer(string ip, int port, Board board)
        {
            _ip = ip;
            _port = port;
            _board = board;
        }

        public void ConnectAndJoin(string name)
        {
            var ep = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(ep);
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => Listen(_cts.Token));

            SendJson(new { Action = "JOIN", Name = name });
        }

        private async Task Listen(CancellationToken ct)
        {
            var buffer = new byte[2048];
            while (!ct.IsCancellationRequested && _client != null && _client.Connected)
            {
                int n = await _client.ReceiveAsync(buffer, SocketFlags.None);
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

                    Handle(line);
                }
            }
        }

        private void Handle(string json)
        {
            using var doc = JsonDocument.Parse(json);
            string action = doc.RootElement.GetProperty("Action").GetString() ?? "";

            switch (action)
            {
                case "INIT":
                    PlayerId = doc.RootElement.GetProperty("PlayerId").GetString() ?? "";
                    Mark = doc.RootElement.GetProperty("Mark").GetString() ?? "?";
                    OnInitReceived?.Invoke(Mark);
                    break;

                case "BOARD":
                    var board = doc.RootElement.GetProperty("Board").Deserialize<int[][]>();
                    var turn = doc.RootElement.GetProperty("NextTurn").GetString() ?? "X";
                    string? winner =
                        doc.RootElement.TryGetProperty("Winner", out var w)
                        && w.ValueKind != JsonValueKind.Null ? w.GetString() : null;

                    if (board != null)
                    {
                        _board.Update(board, turn, winner);
                        OnBoardChanged?.Invoke();
                        if (winner != null)
                            OnWinner?.Invoke(winner);
                    }
                    break;

                case "TIMER":
                    int sec = doc.RootElement.GetProperty("Seconds").GetInt32();
                    string turnNow = doc.RootElement.GetProperty("Turn").GetString() ?? "?";
                    OnTimerUpdate?.Invoke(sec, turnNow);
                    break;

                case "RESET":
                    _board.Reset();
                    OnReset?.Invoke();
                    break;

                // thêm case rank
                case "RANK":
                    int score = doc.RootElement.GetProperty("Score").GetInt32();
                    int wins = doc.RootElement.GetProperty("Wins").GetInt32();
                    int losses = doc.RootElement.GetProperty("Losses").GetInt32();

                    OnRankUpdate?.Invoke(score, wins, losses);
                    break;

                case "NOT_READY":
                    string msg = doc.RootElement.GetProperty("Message").GetString() ?? "Chưa đủ người!";
                    MessageBox.Show(msg, "⚠️ Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;

                case "CHAT":
                    {
                        string player = doc.RootElement.GetProperty("From").GetString() ?? "Unknown";
                        string chatmsg = doc.RootElement.GetProperty("Message").GetString() ?? "";

                        OnChatReceived?.Invoke(player, chatmsg);
                    }
                    break;

                case "SURRENDER":
                    {
                        var winBySurrender = doc.RootElement.GetProperty("Winner").GetString() ?? "";
                        OnWinner?.Invoke(winBySurrender);
                    }
                    break;

                case "LEADERBOARD":
                    {
                        var playersJson = doc.RootElement.GetProperty("Players");
                        var leaderboard = new List<LeaderboardEntry>();
                        
                        foreach (var playerElement in playersJson.EnumerateArray())
                        {
                            leaderboard.Add(new LeaderboardEntry
                            {
                                PlayerId = playerElement.GetProperty("PlayerId").GetString() ?? "",
                                Score = playerElement.GetProperty("Score").GetInt32(),
                                Wins = playerElement.GetProperty("Wins").GetInt32(),
                                Losses = playerElement.GetProperty("Losses").GetInt32()
                            });
                        }

                        OnLeaderboardReceived?.Invoke(leaderboard);
                    }
                    break;
            }
        }

        public void SendMove(int row, int col)
        {
            SendJson(new { Action = "MOVE", PlayerId, X = col, Y = row });
        }

        public void SendChat(string message)
        {
            SendJson(new
            {
                Action = "CHAT",
                PlayerId = PlayerId,
                Message = message
            });
        }// done
        // dau hang
        public void SendSurrender()
        {
            SendJson(new
            {
                Action = "SURRENDER",
                PlayerId = PlayerId
            });
        }// done

        public void RequestLeaderboard()
        {
            SendJson(new { Action = "LEADERBOARD" });
        }

        private void SendJson(object o)
        {
            if (_client == null) return;
            string s = JsonSerializer.Serialize(o) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(s);
            try { _client.Send(data); } catch { }
        }

        public void Disconnect()
        {
            try { _cts?.Cancel(); _client?.Shutdown(SocketShutdown.Both); } catch { }
            try { _client?.Close(); } catch { }
        }


    }
}
