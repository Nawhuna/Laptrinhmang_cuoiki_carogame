using Client.Game;
using Client.Network;
using System.Drawing;
using System.Windows.Forms;

namespace Client.Forms
{
    public partial class FormMain : Form
    {
        // // thanh phan logic !! khong duoc dung vao " Trinh"
        private readonly Board _board = new();
        private GameRenderer _renderer;
        private GameLoop _loop;
        private ConnectToServer _net;
        // thanh phan giao dien
        private Panel _canvas;
        private Label _lblTurn;
        private Label _lblMark;
        private Label _lblTimer;

        // giao dien chinh 
        public FormMain()
        {
            InitializeComponent();
            this.Text = "Caro Online 15x15";
            this.ClientSize = new Size(580, 620);
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

            
            _canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            this.Controls.Add(_canvas);

            // render + loop game
            _renderer = new GameRenderer(_canvas, _board);
            _loop = new GameLoop(this);
            _loop.Start();

            // ham connect to server khong duoc sua
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
                if (sec <= 5) _lblTimer.ForeColor = Color.Red;
                else _lblTimer.ForeColor = Color.Black;
            }));

            _net.OnReset += () => this.BeginInvoke(new Action(() =>
            {
                _lblTurn.Text = "NextTurn: X";
                _lblTimer.Text = "⏱️ 30s";
                _renderer.Refresh();
            }));

            _net.ConnectAndJoin(Environment.UserName);

            // xuw lys click chuot ko duoc dung vao " Trinh"
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
        // xu ly khi close game
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loop?.Stop();
            _net?.Disconnect();
            base.OnFormClosed(e);
        }
    }
}
