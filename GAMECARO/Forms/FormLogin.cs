using System;
using System.Drawing;
using System.Windows.Forms;

namespace Client.Forms
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();

            // ==== FORM STYLE ====
            this.Text = "Đăng nhập trước khi chơi";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(420, 220);
            this.BackColor = Color.WhiteSmoke;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ==== TITLE ====
            Label lblTitle = new Label()
            {
                Text = "Chào mừng bạn đến với Caro Game",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkSlateBlue,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            this.Controls.Add(lblTitle);

            // ==== LABEL NHẬP TÊN ====
            Label lbl = new Label()
            {
                Text = "Nhập tên của bạn:",
                Left = 30,
                Top = 70,
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(lbl);

            // ==== TEXTBOX ====
            TextBox txt = new TextBox()
            {
                Name = "txtName",
                Left = 30,
                Top = 100,
                Width = 350,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(txt);

            // ==== BUTTON ====
            Button btn = new Button()
            {
                Text = "Play",
                Left = 30,
                Top = 140,
                Width = 350,
                Height = 40,
                BackColor = Color.MediumSlateBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btn);

            // ==== SỰ KIỆN CLICK ====
            btn.Click += (s, e) =>
            {
                string name = txt.Text.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Tên không được để trống!");
                    return;
                }

                FormMain fm = new FormMain(name);
                fm.Show();
                this.Hide();
            };
        }
    }
}
