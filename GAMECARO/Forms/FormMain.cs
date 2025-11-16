using System;
using System.Collections.Generic;        // 👈 thêm để dùng List<Point>
using System.Drawing;
using System.Windows.Forms;
using Client.Game;
using Client.Network;

namespace Client.Forms
{
    public partial class FormMain : Form
    {
        private readonly string _playerName;
        // thanh phan logic
        private readonly Board _board = new();
        private GameRenderer _renderer;
        private GameLoop _loop;
        private ConnectToServer _net;

        // thanh phan giao dien
        private Panel _canvas;
        private Label _lblTurn;
        private Label _lblMark;
        private Label _lblTimer;

        // thêm label rank
        private Label _lblScore;
        private Label _lblWins;
        private Label _lblLosses;
    

        // giao dien chinh 

        public FormMain(string playerName)
        {
            _playerName = playerName;

            InitializeComponent();

            this.Text = "Caro Online 15x15";

            this.ClientSize = new Size(800, 620); // rộng hơn tí cho khu chat
            this.DoubleBuffered = true;

            // ====== PANEL TRÊN: TURN / YOU / TIMER ======

            this.ClientSize = new Size(710, 620);   // tăng rộng để đủ rank
            this.DoubleBuffered = true;


            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(8),
                BackColor = Color.LightGray
            };

            _lblTurn = new Label
            {
                AutoSize = true,
                Text = "NextTurn: ...",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            _lblMark = new Label
            {
                AutoSize = true,
                Left = 180,
                Text = $"Player: {_playerName}",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            _lblTimer = new Label
            {
                AutoSize = true,
                Left = 300,
                Text = "⏱️ 30s",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };


            // ================== RANK UI ===================
            _lblScore = new Label
            {
                AutoSize = true,
                Left = 420,
                Text = "Rank: ...",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            _lblWins = new Label
            {
                AutoSize = true,
                Left = 520,
                Text = "W: 0",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            _lblLosses = new Label
            {
                AutoSize = true,
                Left = 600,
                Text = "L: 0",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            // ================== ADD UI ===================

            top.Controls.Add(_lblTurn);
            top.Controls.Add(_lblMark);
            top.Controls.Add(_lblTimer);

            top.Controls.Add(_lblScore);
            top.Controls.Add(_lblWins);
            top.Controls.Add(_lblLosses);

            this.Controls.Add(top);


            // ====== PANEL VẼ BÀN CỜ ======

            _canvas = new Panel
            {
                Dock = DockStyle.Left,
                Width = 380,
                BackColor = Color.White
            };
            this.Controls.Add(_canvas);
            typeof(Panel).GetProperty(
                 "DoubleBuffered",
                  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                )?.SetValue(_canvas, true, null);


            // ====== RENDER + GAME LOOP ======
            _renderer = new GameRenderer(_canvas, _board);
            _loop = new GameLoop(this);
            _loop.Start();

            // ====== KẾT NỐI SERVER (KHÔNG SỬA HÀM CONNECT INSIDE ConnectToServer) ======
            _net = new ConnectToServer("127.0.0.1", 9000, _board);

            _net.OnBoardChanged += () => this.BeginInvoke(new Action(() =>
            {
                _lblTurn.Text = $"NextTurn: {_board.NextTurn}";
                if (_board.Winner != null)
                    _lblTurn.Text += $"  |  Winner: {_board.Winner}";

                // 👉 nếu bạn tự kiểm tra thắng ở client thì có thể
                // gọi hàm ShowWinningLine() ở đây sau khi tìm ra 5 ô.

                _renderer.Refresh();
            }));

            _net.OnWinner += w => this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show($"{w} thắng!", "🎉 Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));

            _net.OnInitReceived += mark => this.BeginInvoke(new Action(() =>
            {
                _lblMark.Text = $"Player: {_playerName} ({mark})";
            }));

            _net.OnTimerUpdate += (sec, turn) => this.BeginInvoke(new Action(() =>
            {
                _lblTimer.Text = $"⏱️ {turn}: {sec}s";
                _lblTimer.ForeColor = sec <= 5 ? Color.Red : Color.Black;
            }));

            _net.OnReset += () => this.BeginInvoke(new Action(() =>
            {
                _lblTurn.Text = "NextTurn: X";
                _lblTimer.Text = "⏱️ 30s";

                // reset gạch đỏ nếu có
                ShowWinningLine(null);

                _renderer.Refresh();
            }));

            // ================== RANK UPDATE ===================
            _net.OnRankUpdate += (score, wins, losses) => this.BeginInvoke(new Action(() =>
            {
                _lblScore.Text = $"Rank: {score}";
                _lblWins.Text = $"W: {wins}";
                _lblLosses.Text = $"L: {losses}";
            }));

            // ================== LEADERBOARD UPDATE ===================
            _net.OnLeaderboardReceived += leaderboard => this.BeginInvoke(new Action(() =>
            {
                ShowLeaderboard(leaderboard);
            }));

            // chat
            _net.OnChatReceived += (player, message) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (player == _playerName) return;
                    AppendChat($"{player}: {message}");
                }));
            };


            _net.ConnectAndJoin(_playerName);

            // xử lý click chuột trên bàn cờ
            _canvas.MouseClick += (s, e) =>
            {
                var cell = _renderer.PointToCell(e.Location);
                if (cell == null || _board.Winner != null) return;

                if (_net.Mark != _board.NextTurn)
                {
                    MessageBox.Show("⏳ Chưa tới lượt của bạn!", "Caro",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _net.SendMove(cell.Value.row, cell.Value.col);
            };
        }

        // 🔴 HÀM MỚI: nhận danh sách 5 ô thắng và bảo renderer vẽ gạch đỏ
        public void ShowWinningLine(List<Point>? winningCells)
        {
            if (winningCells == null || winningCells.Count < 2)
            {
                // xoá gạch đỏ
                _renderer.WinningCells = null;
            }
            else
            {
                _renderer.WinningCells = winningCells;
            }

            _renderer.Refresh();
        }

        // ====== HÀM XỬ LÝ NÚT CONNECT (UI) ======
        private void btnConnect_Click(object? sender, EventArgs e)
        {
            // tạm thời chỉ lấy tên và in ra khung chat
            string name = txtPlayerName.Text.Trim();
            if (string.IsNullOrEmpty(name))
                name = Environment.UserName;

            AppendChat($"[SYSTEM] Bạn đang dùng tên: {name}");

            // TODO: nếu muốn dùng tên này để join server thì sau này
            // có thể sửa _net.ConnectAndJoin(name) và bỏ ConnectAndJoin ở constructor.
        }

        // ====== HÀM XỬ LÝ NÚT SEND CHAT ======
        private void btnSend_Click(object? sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            _net.SendChat(msg);

            // hiện tại mới chat local cho dễ test
            AppendChat($"You: {msg}");
            txtMessage.Clear();

            // TODO: khi có hàm gửi chat trong ConnectToServer, sẽ gọi _net.SendChat(msg) ở đây.
        }
        // dau hàng
        private void btnSurrender_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Bạn có chắc muốn đầu hàng không?",
                "Đầu hàng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                _net.SendSurrender();
            }
        }

        // xem bảng xếp hạng
        private void btnLeaderboard_Click(object? sender, EventArgs e)
        {
            _net.RequestLeaderboard();
        }

        // hiển thị bảng xếp hạng
        private void ShowLeaderboard(List<Client.Network.LeaderboardEntry> leaderboard)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🏆 BẢNG XẾP HẠNG TOP 10 🏆");
            sb.AppendLine("================================");
            sb.AppendLine(string.Format("{0,-5} {1,-15} {2,-8} {3,-8}", "Hạng", "Điểm", "Thắng", "Thua"));
            sb.AppendLine("================================");

            for (int i = 0; i < leaderboard.Count; i++)
            {
                var player = leaderboard[i];
                sb.AppendLine(string.Format("{0,-5} {1,-15} {2,-8} {3,-8}", 
                    $"#{i + 1}", 
                    player.Score, 
                    player.Wins, 
                    player.Losses));
            }

            MessageBox.Show(sb.ToString(), "Bảng xếp hạng", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ====== THÊM DÒNG VÀO KHUNG CHAT ======
        private void AppendChat(string line)
        {
            if (txtChat.InvokeRequired)
            {
                txtChat.Invoke(new Action(() => AppendChat(line)));
            }
            else
            {
                txtChat.AppendText(line + Environment.NewLine);
            }
        }


        // xu ly khi close game

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loop?.Stop();
            _net?.Disconnect();
            base.OnFormClosed(e);
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            // hiện tại chưa cần làm gì khi load
        }
    }
}
