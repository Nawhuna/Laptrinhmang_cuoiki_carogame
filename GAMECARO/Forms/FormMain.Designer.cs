using System.Windows.Forms;
using System.Drawing;

namespace Client.Forms
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer? components = null;

        // 🔹 Các control UI
        private TextBox txtPlayerName;
        private Button btnConnect;
        private Label lblYou;
        private Label lblEnemy;
        private TextBox txtChat;
        private TextBox txtMessage;
        private Button btnSend;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
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
            this.txtPlayerName = new TextBox();
            this.btnConnect = new Button();
            this.lblYou = new Label();
            this.lblEnemy = new Label();
            this.txtChat = new TextBox();
            this.txtMessage = new TextBox();
            this.btnSend = new Button();
            this.SuspendLayout();
            // 
            // txtPlayerName
            // 
            this.txtPlayerName.Location = new Point(400, 50);   // dưới thanh top, bên phải bàn cờ
            this.txtPlayerName.Name = "txtPlayerName";
            this.txtPlayerName.Size = new Size(150, 23);
            this.txtPlayerName.TabIndex = 0;
            this.txtPlayerName.Text = "Nam";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new Point(560, 49);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new Size(75, 23);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblYou
            // 
            this.lblYou.AutoSize = true;
            this.lblYou.Location = new Point(400, 80);
            this.lblYou.Name = "lblYou";
            this.lblYou.Size = new Size(60, 15);
            this.lblYou.TabIndex = 2;
            this.lblYou.Text = "You: N/A";
            // 
            // lblEnemy
            // 
            this.lblEnemy.AutoSize = true;
            this.lblEnemy.Location = new Point(400, 100);
            this.lblEnemy.Name = "lblEnemy";
            this.lblEnemy.Size = new Size(76, 15);
            this.lblEnemy.TabIndex = 3;
            this.lblEnemy.Text = "Enemy: N/A";
            // 
            // txtChat
            // 
            this.txtChat.Location = new Point(400, 130);
            this.txtChat.Multiline = true;
            this.txtChat.Name = "txtChat";
            this.txtChat.ReadOnly = true;
            this.txtChat.ScrollBars = ScrollBars.Vertical;
            this.txtChat.Size = new Size(370, 380);
            this.txtChat.TabIndex = 4;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new Point(400, 520);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new Size(260, 23);
            this.txtMessage.TabIndex = 5;
            // 
            // btnSend
            // 
            this.btnSend.Location = new Point(670, 519);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new Size(100, 23);
            this.btnSend.TabIndex = 6;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 600);   // rộng hơn cho chat
            this.Name = "FormMain";
            this.Text = "Caro Online 15x15";
            this.Load += new System.EventHandler(this.FormMain_Load);

            // 🔹 Thêm control vào form
            this.Controls.Add(this.txtPlayerName);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblYou);
            this.Controls.Add(this.lblEnemy);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
