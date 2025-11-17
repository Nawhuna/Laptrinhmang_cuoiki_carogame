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

        // logic
        private readonly Board _board = new();
        private GameRenderer _renderer;
        private GameLoop _loop;
        private ConnectToServer _net;

        // panel vẽ bàn cờ
        private Panel _canvas;

        // thanh trạng thái trên cùng
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
            this.DoubleBuffered = true;

            // ====== PANEL TRÊN: ICON + TRẢI NGANG TOÀN FORM ======
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(242, 242, 242)
            };

            // đường kẻ dưới panel
            var bottomBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(200, 200, 200)
            };
            top.Controls.Add(bottomBorder);

            var headerFont = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            var headerColor = Color.FromArgb(40, 40, 40);

            // 🎯 NextTurn
            _lblTurn = new Label
            {
                AutoSize = true,
                Location = new Point(10, 12),
                Text = "🎯 NextTurn: ...",
                Font = headerFont,
                ForeColor = headerColor
            };

            // 👤 Player
            _lblMark = new Label
            {
                AutoSize = true,
                Location = new Point(210, 12),
                Text = $"👤 Player: {_playerName}",
                Font = headerFont,
                ForeColor = headerColor
            };

            // ⏱ Timer
            _lblTimer = new Label
            {
                AutoSize = true,
                Location = new Point(430, 12),
                Text = "⏱ 30s",
                Font = headerFont,
                ForeColor = headerColor
            };

            // ⭐ Rank
            _lblScore = new Label
            {
                AutoSize = true,
                Location = new Point(520, 12),
                Text = "⭐ Rank: 1000",
                Font = headerFont,
                ForeColor = headerColor
            };

            // 🏆 W
            _lblWins = new Label
            {
                AutoSize = true,
                Location = new Point(650, 12),
                Text = "🏆 W: 0",
                Font = headerFont,
                ForeColor = headerColor
            };

            // ❌ L
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

            // thêm top panel vào form (trên hết, phủ rộng toàn form)
            this.Controls.Add(top);

            // ====== PANEL VẼ BÀN CỜ BÊN TRÁI (DƯỚI HEADER) ======
            int boardWidth = 380;
            int headerHeight = top.Height;

            _canvas = new Panel
            {
                Location = new Point(0, headerHeight), // ngay dưới thanh trên cùng, sát trái
                Size = new Size(boardWidth, this.ClientSize.Height - headerHeight),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(_canvas);

            // bật double buffer cho panel vẽ
            typeof(Panel).GetProperty(
                 "DoubleBuffered",
                  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                )?.SetValue(_canvas, true, null);

            // ====== RENDER + GAME LOOP ======
            _renderer = new GameRenderer(_canvas, _board);
            _loop = new GameLoop(this);
            _loop.Start();

            // ====== KẾT NỐI SERVER ======
            _net = new ConnectToServer("127.0.0.1", 9000, _board);

            _net.OnBoardChanged += () => this.BeginInvoke(new Action(() =>
            {
                _lblTurn.Text = $"🎯 NextTurn: {_board.NextTurn}";
                if (_board.Winner != null)
                    _lblTurn.Text += $"  |  Winner: {_board.Winner}";

                _renderer.Refresh();
            }));

            _net.OnWinner += w => this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show($"{w} thắng!", "🎉 Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));

            _net.OnInitReceived += mark => this.BeginInvoke(new Action(() =>
            {
                _lblMark.Text = $"👤 Player: {_playerName} ({mark})";
            }));

            _net.OnTimerUpdate += (sec, turn) => this.BeginInvoke(new Action(() =>
            {
                _lblTimer.Text = $"⏱ {turn}: {sec}s";
                _lblTimer.ForeColor = sec <= 5 ? Color.Red : headerColor;
            }));

            _net.OnReset += () => this.BeginInvoke(new Action(() =>
            {
                _renderer.WinningCells = null;          // 🔹 xoá 5 ô thắng cũ
                _lblTurn.Text = "🎯 NextTurn: X";
                _lblTimer.Text = "⏱ 30s";
                _renderer.Refresh();
            }));

            _net.OnRankUpdate += (score, wins, losses) => this.BeginInvoke(new Action(() =>
            {
                _lblScore.Text = $"⭐ Rank: {score}";
                _lblWins.Text = $"🏆 W: {wins}";
                _lblLosses.Text = $"❌ L: {losses}";
            }));

            // chat từ server
            _net.OnChatReceived += (player, message) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (player == _playerName) return;
                    AppendChat($"{player}: {message}");
                }));
            };

            // auto join luôn với tên đã truyền vào
            _net.ConnectAndJoin(_playerName);

            // click chuột xuống ô cờ
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

        // dùng để vẽ line thắng (nếu server gửi list ô thắng)
        public void ShowWinningLine(List<Point>? winningCells)
        {
            _renderer.WinningCells = winningCells;
            _renderer.Refresh();
        }

        // ====== NÚT GỬI CHAT ======
        private void btnSend_Click(object? sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            _net.SendChat(msg);
            AppendChat($"You: {msg}");
            txtMessage.Clear();
        }

        // ====== NÚT ĐẦU HÀNG ======
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

        // thêm dòng vào khung chat
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loop?.Stop();
            _net?.Disconnect();
            base.OnFormClosed(e);
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            // Tô màu chữ CARO GAME bằng RichTextBox
            string text = "C A R O  G A M E";   // có khoảng cách giống hình bạn gửi
            lblTitle.Text = text;

            int letterIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                    continue; // bỏ qua khoảng trắng

                lblTitle.SelectionStart = i;
                lblTitle.SelectionLength = 1;

                // xen kẽ đỏ – xanh
                lblTitle.SelectionColor = (letterIndex % 2 == 0)
                    ? Color.Red
                    : Color.Green;

                letterIndex++;
            }

            // bỏ select
            lblTitle.SelectionLength = 0;
        }
    }
}
