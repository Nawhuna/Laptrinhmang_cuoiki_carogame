using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Client.Game;
using Client.Network;

namespace Client.Forms
{
    public partial class FormMain : Form
    {
        private readonly string _playerName;

        // Logic game
        private readonly Board _board = new();
        private GameRenderer _renderer;
        private GameLoop _loop;
        private ConnectToServer _net;

        // Panel vẽ bàn cờ
        private Panel _canvas;

        // Các label trạng thái trên thanh trên cùng
        private Label _lblTurn;
        private Label _lblMark;
        private Label _lblTimer;
        private Label _lblScore;
        private Label _lblWins;
        private Label _lblLosses;

        public FormMain(string playerName)
        {
            _playerName = playerName;

            InitializeComponent();

            this.Text = "Caro Online 15x15";
            this.ClientSize = new Size(800, 620);
            this.DoubleBuffered = true; // giảm nhấp nháy cho Form

            // Panel trên cùng chứa thông tin trạng thái
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(242, 242, 242)
            };

            // Đường kẻ mảnh dưới panel trên
            var bottomBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            top.Controls.Add(bottomBorder);

            var headerFont = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            var headerColor = Color.FromArgb(40, 40, 40);

            // Lượt đi hiện tại
            _lblTurn = new Label
            {
                AutoSize = true,
                Location = new Point(10, 12),
                Text = "🎯 NextTurn: ...",
                Font = headerFont,
                ForeColor = headerColor
            };

            // Tên người chơi + dấu X/O
            _lblMark = new Label
            {
                AutoSize = true,
                Location = new Point(210, 12),
                Text = $"👤 Player: {_playerName}",
                Font = headerFont,
                ForeColor = headerColor
            };

            // Thời gian còn lại cho lượt hiện tại
            _lblTimer = new Label
            {
                AutoSize = true,
                Location = new Point(430, 12),
                Text = "⏱ 30s",
                Font = headerFont,
                ForeColor = headerColor
            };

            // Điểm Rank ban đầu
            _lblScore = new Label
            {
                AutoSize = true,
                Location = new Point(520, 12),
                Text = "⭐ Rank: 1000",
                Font = headerFont,
                ForeColor = headerColor
            };

            // Số trận thắng
            _lblWins = new Label
            {
                AutoSize = true,
                Location = new Point(650, 12),
                Text = "🏆 W: 0",
                Font = headerFont,
                ForeColor = headerColor
            };

            // Số trận thua
            _lblLosses = new Label
            {
                AutoSize = true,
                Location = new Point(720, 12),
                Text = "❌ L: 0",
                Font = headerFont,
                ForeColor = headerColor
            };

            top.Controls.AddRange(new Control[]
            {
                _lblTurn, _lblMark, _lblTimer, _lblScore, _lblWins, _lblLosses
            });

            this.Controls.Add(top);

            // Panel bàn cờ nằm bên trái, bên dưới thanh trạng thái
            int boardWidth = 380;
            int headerHeight = top.Height;

            _canvas = new Panel
            {
                Location = new Point(0, headerHeight),
                Size = new Size(boardWidth, this.ClientSize.Height - headerHeight),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(_canvas);

            // Bật double buffer cho panel vẽ để di chuyển / redraw mượt hơn
            typeof(Panel).GetProperty(
                 "DoubleBuffered",
                  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                )?.SetValue(_canvas, true, null);

            // Khởi tạo renderer và game loop để vẽ + cập nhật màn hình
            _renderer = new GameRenderer(_canvas, _board);
            _loop = new GameLoop(this);
            _loop.Start();

            // Kết nối tới server TCP
            _net = new ConnectToServer("127.0.0.1", 9000, _board);

            // Server gửi trạng thái bàn cờ mới
            _net.OnBoardChanged += () => this.BeginInvoke(new Action(() =>
            {
                _lblTurn.Text = $"🎯 NextTurn: {_board.NextTurn}";
                if (_board.Winner != null)
                    _lblTurn.Text += $"  |  Winner: {_board.Winner}";

                _renderer.Refresh();
            }));

            // Khi có người thắng (thông báo popup)
            _net.OnWinner += w => this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show($"{w} thắng!", "🎉 Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));

            // Nhận thông tin ban đầu: id người chơi + X/O
            _net.OnInitReceived += mark => this.BeginInvoke(new Action(() =>
            {
                _lblMark.Text = $"👤 Player: {_playerName} ({mark})";
            }));

            // Cập nhật timer lượt chơi
            _net.OnTimerUpdate += (sec, turn) => this.BeginInvoke(new Action(() =>
            {
                _lblTimer.Text = $"⏱ {turn}: {sec}s";
                _lblTimer.ForeColor = sec <= 5 ? Color.Red : headerColor;
            }));

            // Server yêu cầu reset ván mới
            _net.OnReset += () => this.BeginInvoke(new Action(() =>
            {
                _renderer.WinningCells = null;   // xoá highlight 5 ô thắng cũ
                _lblTurn.Text = "🎯 NextTurn: X";
                _lblTimer.Text = "⏱ 30s";
                _renderer.Refresh();
            }));

            // Server gửi thông tin Rank/W/L
            _net.OnRankUpdate += (score, wins, losses) => this.BeginInvoke(new Action(() =>
            {
                _lblScore.Text = $"⭐ Rank: {score}";
                _lblWins.Text = $"🏆 W: {wins}";
                _lblLosses.Text = $"❌ L: {losses}";
            }));

            // Nhận tin nhắn chat từ server
            _net.OnChatReceived += (player, message) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (player == _playerName) return; // không echo lại tin của mình
                    AppendChat($"{player}: {message}");
                }));
            };

            // Gửi yêu cầu join phòng với tên người chơi
            _net.ConnectAndJoin(_playerName);

            // Click chuột trên bàn cờ để đánh quân
            _canvas.MouseClick += (s, e) =>
            {
                var cell = _renderer.PointToCell(e.Location);
                if (cell == null || _board.Winner != null) return;

                // Kiểm tra có đúng lượt mình không
                if (_net.Mark != _board.NextTurn)
                {
                    MessageBox.Show("⏳ Chưa tới lượt của bạn!", "Caro",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _net.SendMove(cell.Value.row, cell.Value.col);
            };
        }

        // Cho phép set WinningCells từ ngoài (hiện tại không dùng tới)
        public void ShowWinningLine(List<Point>? winningCells)
        {
            _renderer.WinningCells = winningCells;
            _renderer.Refresh();
        }

        // Nút gửi chat
        private void btnSend_Click(object? sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            _net.SendChat(msg);
            AppendChat($"You: {msg}");
            txtMessage.Clear();
        }

        // Nút đầu hàng
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

        // Thêm 1 dòng vào khung chat (thread-safe)
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

        // Đóng form → dừng loop & ngắt kết nối server
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loop?.Stop();
            _net?.Disconnect();
            base.OnFormClosed(e);
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            // Đổi màu từng chữ trong "C A R O  G A M E" (đỏ / xanh xen kẽ)
            string text = "C A R O  G A M E";
            lblTitle.Text = text;

            int letterIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                    continue;

                lblTitle.SelectionStart = i;
                lblTitle.SelectionLength = 1;

                lblTitle.SelectionColor = (letterIndex % 2 == 0)
                    ? Color.Red
                    : Color.Green;

                letterIndex++;
            }

            // Bỏ trạng thái select text
            lblTitle.SelectionLength = 0;
        }
    }
}
