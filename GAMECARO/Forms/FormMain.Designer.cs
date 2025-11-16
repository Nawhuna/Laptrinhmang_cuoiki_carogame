using System.Drawing;
using System.Windows.Forms;

namespace Client.Forms
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // các control bên phải
        private Panel panelTitle;
        private TextBox txtChat;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnSurrender;

        /// <summary>
        /// Clean up any resources being used.
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
            this.components = new System.ComponentModel.Container();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnSurrender = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelTitle  (CARO GAME)
            // 
            this.panelTitle.Location = new System.Drawing.Point(380, 50);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(430, 55);
            this.panelTitle.BackColor = System.Drawing.Color.Transparent;
            this.panelTitle.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelTitle_Paint);
            // 
            // txtChat
            // 
            this.txtChat.Location = new System.Drawing.Point(400, 110);
            this.txtChat.Multiline = true;
            this.txtChat.Name = "txtChat";
            this.txtChat.ReadOnly = true;
            this.txtChat.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtChat.Size = new System.Drawing.Size(370, 380);
            this.txtChat.TabIndex = 0;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(400, 520);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(260, 23);
            this.txtMessage.TabIndex = 1;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(670, 516);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 30);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Gửi";
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnSurrender
            // 
            this.btnSurrender.Location = new System.Drawing.Point(670, 552);
            this.btnSurrender.Name = "btnSurrender";
            this.btnSurrender.Size = new System.Drawing.Size(100, 30);
            this.btnSurrender.TabIndex = 3;
            this.btnSurrender.Text = "Đầu hàng";
            this.btnSurrender.BackColor = System.Drawing.Color.FromArgb(220, 0, 0);
            this.btnSurrender.ForeColor = System.Drawing.Color.White;
            this.btnSurrender.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSurrender.FlatAppearance.BorderSize = 0;
            this.btnSurrender.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSurrender.UseVisualStyleBackColor = false;
            this.btnSurrender.Click += new System.EventHandler(this.btnSurrender_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMain";
            this.Text = "Caro Online 15x15";
            this.Load += new System.EventHandler(this.FormMain_Load);

            // thêm control vào form
            this.Controls.Add(this.panelTitle);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnSurrender);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
