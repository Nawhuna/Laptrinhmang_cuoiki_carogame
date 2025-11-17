using System.Windows.Forms;
using System.Drawing;

namespace Client.Forms
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer? components = null;

        // UI bên phải
        private RichTextBox lblTitle;
        private TextBox txtChat;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnSurrender;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new RichTextBox();
            txtChat = new TextBox();
            txtMessage = new TextBox();
            btnSend = new Button();
            btnSurrender = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.FromArgb(242, 242, 242);
            lblTitle.BorderStyle = BorderStyle.None;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Location = new Point(491, 67);
            lblTitle.Margin = new Padding(3, 4, 3, 4);
            lblTitle.Multiline = false;
            lblTitle.Name = "lblTitle";
            lblTitle.ReadOnly = true;
            lblTitle.ScrollBars = RichTextBoxScrollBars.None;
            lblTitle.Size = new Size(366, 53);
            lblTitle.TabIndex = 0;
            lblTitle.TabStop = false;
            lblTitle.Text = "";
            // 
            // txtChat
            // 
            txtChat.Location = new Point(457, 133);
            txtChat.Margin = new Padding(3, 4, 3, 4);
            txtChat.Multiline = true;
            txtChat.Name = "txtChat";
            txtChat.ReadOnly = true;
            txtChat.ScrollBars = ScrollBars.Vertical;
            txtChat.Size = new Size(422, 505);
            txtChat.TabIndex = 1;
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(457, 693);
            txtMessage.Margin = new Padding(3, 4, 3, 4);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(297, 27);
            txtMessage.TabIndex = 2;
            // 
            // button send
            // 
            btnSend.BackColor = Color.FromArgb(60, 60, 60);
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Location = new Point(766, 692);
            btnSend.Margin = new Padding(3, 4, 3, 4);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(114, 40);
            btnSend.TabIndex = 3;
            btnSend.Text = "Gửi";
            btnSend.UseVisualStyleBackColor = false;
            btnSend.Click += btnSend_Click;
            // 
            // button dau hang 
            // 
            btnSurrender.BackColor = Color.FromArgb(220, 0, 0);
            btnSurrender.FlatAppearance.BorderSize = 0;
            btnSurrender.FlatStyle = FlatStyle.Flat;
            btnSurrender.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSurrender.ForeColor = Color.White;
            btnSurrender.Location = new Point(766, 740);
            btnSurrender.Margin = new Padding(3, 4, 3, 4);
            btnSurrender.Name = "btnSurrender";
            btnSurrender.Size = new Size(114, 40);
            btnSurrender.TabIndex = 4;
            btnSurrender.Text = "Đầu hàng";
            btnSurrender.UseVisualStyleBackColor = false;
            btnSurrender.Click += btnSurrender_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(606, 800);
            Controls.Add(lblTitle);
            Controls.Add(txtChat);
            Controls.Add(txtMessage);
            Controls.Add(btnSend);
            Controls.Add(btnSurrender);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormMain";
            Text = "Caro Online 15x15";
            Load += FormMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
