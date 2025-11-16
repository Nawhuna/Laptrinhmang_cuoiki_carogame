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

        public FormMain()
        {
            // tạm thời dùng tên máy / user làm playerName
            _playerName = Environment.UserName;

            InitializeComponent();

            this.Text = "Caro Online 15x15";
            this.ClientSize = new Size(800, 620);
            this.DoubleBuffered = true;

            // ===== HEADER TRÊN: NEXTTURN / PLAYER / TIME / RANK / W/L =====
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(242, 242, 242)
            };

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
                Location = new Point(200, 12),
                Text = $"👤 Player: {_playerName}",
                Font = headerFont,
                ForeColor = headerColor
            };

            // ⏱ Timer
            _lblTimer = new Label
            {
                AutoSize = true,
                Location = new Point(420, 12),
                Text = "⏱ 30s",
                Font = headerFont,
                ForeColor = headerColor
            };

            // ⭐ Rank
            _lblScore = new Label
            {
                AutoSize = true,
                Location = new Point(520, 12),
                Text = "⭐ Rank: ...",
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

            // đường kẻ dưới header
            var bottomBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(200, 200, 200)
            };

            top.Controls.AddRange(new Control[]
            {
                _lblTurn, _lblMark, _lblTimer, _lblScore, _lblWins, _lblLosses, bottomBorder
            });

            // ===== PANEL VẼ BÀN CỜ BÊN TRÁI (DƯỚI HEADER) =====
            int boardWidth = 380;
            int headerHeight = top.Height;

            _canvas = new Panel
            {
                Location = new Point(0, headerHeight),
                Size = new Size(boardWidth, this.ClientSize.Height - headerHeight),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            // Thêm control theo đúng thứ tự z-order
            this.Controls.Add(_canvas);   // canvas ở dưới
            this.Controls.Add(top);       // header nằm trên cùng

            // bật double-buffer cho canvas và panelTitle để đỡ nhấp nháy
            typeof(Panel).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            )?.SetValue(_canvas, true, null);

            typeof(Panel).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            )?.SetValue(panelTitle, true, null);

            // đảm bảo header + CARO GAME nằm trên cùng
            top.BringToFront();
            panelTitle.BringToFront();

            // ===== RENDER + GAME LOOP =====
            _renderer = new GameRenderer(_canvas, _board);
            _loop = new GameLoop(this);
            _loop.Start();

            // ===== KẾT NỐI SERVER (chỉ dùng các event chắc chắn có) =====
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

            // auto join với tên hiện tại
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

        private void PanelTitle_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            string text = "Ｃ Ａ Ｒ Ｏ   Ｇ Ａ Ｍ Ｅ";
            using Font font = new Font("Segoe UI", 20, FontStyle.Bold);

            SizeF size = g.MeasureString(text, font);

            // 🔥 DỊCH CHỮ QUA TRÁI 20PX CHO ĐẸP
            float x = (panelTitle.Width - size.Width) / 2 - 20;
            float y = 5;

            Color red = Color.FromArgb(220, 20, 20);
            Color green = Color.FromArgb(0, 180, 80);

            bool useRed = true;
            float drawX = x;

            foreach (char c in text)
            {
                string s = c.ToString();
                SizeF charSize = g.MeasureString(s, font);

                using (var brush = new SolidBrush(useRed ? red : green))
                    g.DrawString(s, font, brush, drawX, y);

                drawX += charSize.Width;
                if (c != ' ') useRed = !useRed;
            }

            // ❌ BỎ GẠCH CHÂN
        }


        // ===== NÚT GỬI CHAT =====
        private void btnSend_Click(object? sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            // tạm thời chỉ chat local trong khung chat
            AppendChat($"You: {msg}");
            txtMessage.Clear();

            // khi nào ConnectToServer có hàm SendChat, có thể thêm:
            // _net.SendChat(msg);
        }

        // ===== NÚT ĐẦU HÀNG =====
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
                AppendChat("[SYSTEM] Bạn đã đầu hàng.");

                // Khi nào ConnectToServer có hàm SendSurrender, có thể thêm:
                // _net.SendSurrender();
            }
        }

        // thêm dòng vào khung chat
        private void AppendChat(string line)
        {
            if (txtChat == null) return;

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
            // hiện tại chưa cần làm gì khi load
        }
    }
}
