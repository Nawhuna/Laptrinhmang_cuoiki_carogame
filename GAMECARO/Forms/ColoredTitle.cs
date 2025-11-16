using System.Drawing;
using System.Windows.Forms;

namespace Client.Forms
{
    public class ColoredTitle : Control
    {
        private readonly Color red = Color.FromArgb(220, 20, 20);
        private readonly Color green = Color.FromArgb(0, 180, 80);

        private readonly string title = "Ｃ Ａ Ｒ Ｏ   Ｇ Ａ Ｍ Ｅ";

        public ColoredTitle()
        {
            this.DoubleBuffered = true;
            this.Height = 45;
            this.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // trung tâm hoá
            SizeF size = g.MeasureString(title, this.Font);
            float x = (this.Width - size.Width) / 2;
            float y = 5;

            // Vẽ từng ký tự với 2 màu xen kẽ
            bool useRed = true;
            float drawX = x;

            foreach (char c in title)
            {
                string s = c.ToString();
                SizeF charSize = g.MeasureString(s, this.Font);

                using (var brush = new SolidBrush(useRed ? red : green))
                {
                    g.DrawString(s, this.Font, brush, drawX, y);
                }

                drawX += charSize.Width;
                if (c != ' ') useRed = !useRed; // đổi màu mỗi ký tự không phải khoảng trắng
            }

            // Vẽ line ngang
            using (var pen = new Pen(Color.FromArgb(180, 180, 180), 2))
            {
                g.DrawLine(pen, this.Width * 0.15f, this.Height - 5,
                                this.Width * 0.85f, this.Height - 5);
            }
        }
    }
}
