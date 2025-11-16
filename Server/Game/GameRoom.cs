using System.Text.Json;
using System.Timers;
using Server.Model;
using Server.Game;

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

        private System.Timers.Timer? _turnTimer;
        private int _remainingTime = 30;

        // auto move đếm số lần tự đánh
        private int _autoMoves = 0;

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
            if (_players.Count < 2)
            {
                Console.WriteLine($"⏳ Phòng {Id} chưa đủ 2 người → chưa thể bắt đầu ván mới.");
                return;
            }
            Array.Clear(_board, 0, _board.Length);
            _winner = null;
            _nextTurn = "X";
            _autoMoves = 0;

            Console.WriteLine($"🔄 Phòng {Id} bắt đầu ván mới!");
            BroadcastBoard();
            StartTurnTimer();
        }

        // time 30s
        private void StartTurnTimer()
        {

            if (_players.Count < 2)
            {
                Console.WriteLine($"⏳ Phòng {Id} chưa đủ 2 người → không chạy timer.");
                return;
            }

            _turnTimer?.Stop();
            _remainingTime = 30;

            _turnTimer = new System.Timers.Timer(1000);
            _turnTimer.AutoReset = true; 
            _turnTimer.Elapsed += (s, e) =>
            {
                _remainingTime--;
                BroadcastTimer();

                if (_remainingTime <= 0)
                {
                    _turnTimer.Stop();
                    Console.WriteLine($"⏰ {_nextTurn} hết thời gian → auto move");
                    AutoMove();
                }
            };

            _turnTimer.Start();
        } // done

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

        // auto đánh thay người chơi hết giờ
        private void AutoMove()
        {
            int mark = _nextTurn == "X" ? 1 : 2;
            int enemy = mark == 1 ? 2 : 1;

            if (FindBlockMove(enemy, out int br, out int bc))
            {
                MakeAutoMove(br, bc, mark);
                return;
            }

            if (FindNearMove(mark, out int nr, out int nc))
            {
                MakeAutoMove(nr, nc, mark);
                return;
            }

            MakeAutoMove(Size / 2, Size / 2, mark);
        }

        private void MakeAutoMove(int r, int c, int mark)
        {
            if (_board[r, c] == 0)
            {
                _board[r, c] = mark;
                _autoMoves++;

                Console.WriteLine($"AutoMove cho {_nextTurn}: [{r},{c}]  ({_autoMoves}/3)");

                if (_autoMoves >= 3)
                {
                    string loser = _nextTurn;
                    _winner = loser == "X" ? "O" : "X";

                    UpdateRankAfterMatch(_winner);
                    BroadcastBoard();

                    Task.Delay(3000).ContinueWith(_ => StartNewGame());
                    return;
                }

                _nextTurn = _nextTurn == "X" ? "O" : "X";
                BroadcastBoard();
                StartTurnTimer();
            }
        }

        private bool FindBlockMove(int enemy, out int br, out int bc)
        {
            int[][] dirs =
            {
                new[]{1,0},
                new[]{0,1},
                new[]{1,1},
                new[]{1,-1}
            };

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (_board[r, c] != enemy) continue;

                    foreach (var d in dirs)
                    {
                        int rr = r + d[0];
                        int cc = c + d[1];

                        if (rr >= 0 && rr < Size && cc >= 0 && cc < Size)
                        {
                            if (_board[rr, cc] == 0)
                            {
                                br = rr;
                                bc = cc;
                                return true;
                            }
                        }
                    }
                }
            }

            br = bc = -1;
            return false;
        }

        private bool FindNearMove(int mark, out int nr, out int nc)
        {
            int[][] around =
            {
                new[]{1,0}, new[]{-1,0},
                new[]{0,1}, new[]{0,-1},
                new[]{1,1}, new[]{1,-1},
                new[]{-1,1}, new[]{-1,-1}
            };

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (_board[r, c] == mark)
                    {
                        foreach (var a in around)
                        {
                            int rr = r + a[0];
                            int cc = c + a[1];

                            if (rr >= 0 && rr < Size && cc >= 0 && cc < Size)
                            {
                                if (_board[rr, cc] == 0)
                                {
                                    nr = rr;
                                    nc = cc;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            nr = nc = -1;
            return false;
        }

        // ham logic ko duoc dung vao " trinh"
        public void HandleMove(string pid, int x, int y)
        {
            if (_players.Count < 2)
            {
                Console.WriteLine($"⚠️ Phòng {Id} chưa đủ 2 người → không cho đánh.");

                var warn = new
                {
                    Action = "NOT_READY",
                    Message = "Phòng chưa đủ 2 người. Không thể đánh!"
                };
                // gui cho thang click
                var p = _players.FirstOrDefault(x => x.PlayerId == pid);
                p?.SendJsonLine?.Invoke(JsonSerializer.Serialize(warn));

                return;
            }

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

                // cập nhật rank
                UpdateRankAfterMatch(_winner);

                BroadcastBoard();

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

        // rank
        private void UpdateRankAfterMatch(string winner)
        {
            var pX = _players.ElementAtOrDefault(0);
            var pO = _players.ElementAtOrDefault(1);

            if (pX == null || pO == null) return;

            if (winner == "X")
            {
                RankManager.AddWin(pX.PlayerId);
                RankManager.AddLoss(pO.PlayerId);
            }
            else
            {
                RankManager.AddWin(pO.PlayerId);
                RankManager.AddLoss(pX.PlayerId);
            }

            BroadcastRank();
        }

        private void BroadcastRank()
        {
            foreach (var p in _players)
            {
                var info = RankManager.Get(p.PlayerId);

                var msg = new
                {
                    Action = "RANK",
                    Score = info.Score,
                    Wins = info.Wins,
                    Losses = info.Losses
                };

                p.SendJsonLine?.Invoke(JsonSerializer.Serialize(msg));
            }
        }

        private int MarkOf(string pid)
        {
            if (_players.Count == 0) return 0;
            return _players.First().PlayerId == pid ? 1 : 2;
        }
        // chat
        public void BroadcastChat(string playerId, string message)
        {
            var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
            string name = player?.Name ?? "Unknown";

            var msgObj = new
            {
                Action = "CHAT",
                From = name,
                Message = message
            };

            string json = JsonSerializer.Serialize(msgObj);

            foreach (var p in _players)
                p.SendJsonLine?.Invoke(json);

            Console.WriteLine($" Chat [{name}]: {message}");
        }
        // gg
        public void HandleSurrender(string pid)
        {
            if (_players.Count < 2) return;
            if (_winner != null) return;

            var loser = _players.FirstOrDefault(p => p.PlayerId == pid);
            if (loser == null) return;

            string loserMark = MarkOf(pid) == 1 ? "X" : "O";
            string winnerMark = loserMark == "X" ? "O" : "X";

            _winner = winnerMark;

            Console.WriteLine($"🏳️ Người chơi {loser.Name} đầu hàng → {winnerMark} thắng!");

            _turnTimer?.Stop();

            UpdateRankAfterMatch(_winner);


            var msg = new
            {
                Action = "SURRENDER",
                Loser = loser.Name,
                Winner = winnerMark
            };

            string json = JsonSerializer.Serialize(msg);
            foreach (var p in _players)
                p.SendJsonLine?.Invoke(json);
            BroadcastBoard();

            Task.Delay(3000).ContinueWith(_ => StartNewGame());
        }


        private string MarkSymbol(int mark) => mark == 1 ? "X" : "O";
    }
}
