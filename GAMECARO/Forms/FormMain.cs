using System;
using System.Drawing;
using System.Windows.Forms;
using Client.Game;
using Client.Network;

namespace Client.Forms
{
    public partial class FormMain : Form
    {
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

        public FormMain()
        {
            InitializeComponent();

            this.Text = "Caro Online 15x15";
            this.ClientSize = new Size(800, 620); // rộng hơn tí cho khu chat
            this.DoubleBuffered = true;

            // ====== PANEL TRÊN: TURN / YOU / TIMER ======
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
                Left = 200,
                Text = "You: ?",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            _lblTimer = new Label
            {
                AutoSize = true,
                Left = 400,
                Text = "⏱️ 30s",
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            top.Controls.Add(_lblTurn);
            top.Controls.Add(_lblMark);
            top.Controls.Add(_lblTimer);
            this.Controls.Add(top);

            // ====== PANEL VẼ BÀN CỜ ======
            _canvas = new Panel
            {
                Dock = DockStyle.Left,
                Width = 380,
                BackColor = Color.White
            };
            this.Controls.Add(_canvas);

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
                _renderer.Refresh();
            }));

            _net.OnWinner += w => this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show($"{w} thắng!", "🎉 Kết quả",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));

            _net.OnInitReceived += mark => this.BeginInvoke(new Action(() =>
            {
                _lblMark.Text = $"You: {mark}";
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
                _renderer.Refresh();
            }));

            // hiện tại vẫn dùng tên mặc định của Windows
            _net.ConnectAndJoin(Environment.UserName);

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

            // hiện tại mới chat local cho dễ test
            AppendChat($"You: {msg}");
            txtMessage.Clear();

            // TODO: khi có hàm gửi chat trong ConnectToServer, sẽ gọi _net.SendChat(msg) ở đây.
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

        // xử lý khi close game
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
