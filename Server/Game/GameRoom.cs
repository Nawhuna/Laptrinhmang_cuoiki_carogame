using System.Text.Json;
using System.Timers;

namespace Server.Game
{
    public class GameRoom
    {
        public string Id { get; }
        private const int Size = 15;
        private readonly int[,] _board = new int[Size, Size];
        private readonly List<ClientRef> _players = new();
        private string _nextTurn = "X";
        private string? _winner = null;
        // diem time
        private System.Timers.Timer? _turnTimer;
        private int _remainingTime = 30;

        public GameRoom(string id) => Id = id;

        public bool CanJoin => _players.Count < 2;
        public bool IsEmpty => _players.Count == 0;

        public void Join(ClientRef c)
        {
            if (_players.Count >= 2)
                throw new InvalidOperationException("Phòng đã đầy!");

            _players.Add(c);
            string mark = _players.Count == 1 ? "X" : "O";

            var init = new
            {
                Action = "INIT",
                PlayerId = c.PlayerId,
                Mark = mark
            };
            c.SendJsonLine?.Invoke(JsonSerializer.Serialize(init));
            Console.WriteLine($"👤 {c.Name} vào phòng {Id} là {mark}");

            if (_players.Count == 2)
                StartNewGame();
        }

        public void Leave(string pid)
        {
            var p = _players.FirstOrDefault(x => x.PlayerId == pid);
            if (p != null)
            {
                _players.Remove(p);
                Console.WriteLine($"🚪 {p.Name} rời phòng {Id}");
            }
        }

        private void StartNewGame()
        {
            Array.Clear(_board, 0, _board.Length);
            _winner = null;
            _nextTurn = "X";
            Console.WriteLine($"🔄 Phòng {Id} bắt đầu ván mới!");
            BroadcastBoard();
            StartTurnTimer();
        }
        // time 30s
        private void StartTurnTimer()
        {
            _turnTimer?.Stop();
            _remainingTime = 30;
            _turnTimer = new System.Timers.Timer(1000);
            _turnTimer.Elapsed += (s, e) =>
            {
                _remainingTime--;
                BroadcastTimer();

                if (_remainingTime <= 0)
                {
                    _turnTimer.Stop();
                    string loser = _nextTurn;
                    _winner = (loser == "X") ? "O" : "X";
                    Console.WriteLine($"⏰ {_winner} thắng vì {_nextTurn} hết thời gian!");
                    BroadcastBoard();

                    // Reset sau 3s
                    Task.Delay(3000).ContinueWith(_ => StartNewGame());
                }
            };
            _turnTimer.Start();
        } // done

        // gui time con lai
        private void BroadcastTimer()
        {
            var msg = new
            {
                Action = "TIMER",
                Seconds = _remainingTime,
                Turn = _nextTurn
            };
            string json = JsonSerializer.Serialize(msg);
            foreach (var p in _players)
                p.SendJsonLine?.Invoke(json);
        }

        // ham logic ko duoc dung vao " trinh"
        public void HandleMove(string pid, int x, int y)
        {
            if (_winner != null) return;
            if (x < 0 || y < 0 || x >= Size || y >= Size) return;

            int mark = MarkOf(pid);
            if ((_nextTurn == "X" && mark != 1) || (_nextTurn == "O" && mark != 2)) return;
            if (_board[y, x] != 0) return;

            _board[y, x] = mark;

            if (CaroLogic.CheckWin(_board, Size, y, x, mark))
            {
                _winner = MarkSymbol(mark);
                Console.WriteLine($"🎉 Người thắng phòng {Id}: {_winner}");
                _turnTimer?.Stop();
                BroadcastBoard();

                // reset game sau 3s
                Task.Delay(3000).ContinueWith(_ => StartNewGame());
            }
            else
            {
                _nextTurn = (_nextTurn == "X") ? "O" : "X";
                BroadcastBoard();
                StartTurnTimer(); 
            }
        }

        // cap nhat trang thai cho tat ca cac thang trong room 
        private void BroadcastBoard()
        {
            var boardJagged = new int[Size][];
            for (int r = 0; r < Size; r++)
            {
                boardJagged[r] = new int[Size];
                for (int c = 0; c < Size; c++)
                    boardJagged[r][c] = _board[r, c];
            }

            var msg = new
            {
                Action = "BOARD",
                Board = boardJagged,
                NextTurn = _nextTurn,
                Winner = _winner
            };
            string json = JsonSerializer.Serialize(msg);
            foreach (var p in _players)
                p.SendJsonLine?.Invoke(json);
        }

        // kiem tra thang client di x hay o
        private int MarkOf(string pid)
        {
            if (_players.Count == 0) return 0;
            return _players.First().PlayerId == pid ? 1 : 2;
        }

        private string MarkSymbol(int mark) => mark == 1 ? "X" : "O";
    }
}
