using System.Drawing;
using System.Windows.Forms;

namespace Client.Game
{
    public class GameRenderer
    {
        private readonly Panel _canvas;
        private readonly Board _board;
        private const int CellSize = 35;

        public GameRenderer(Panel canvas, Board board)
        {
            _canvas = canvas;
            _board = board;
            _canvas.Paint += Canvas_Paint;
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            Pen gridPen = Pens.Gray;

            // Vẽ lưới 
            for (int i = 0; i <= Board.Size; i++)
            {
                g.DrawLine(gridPen, i * CellSize, 0, i * CellSize, Board.Size * CellSize);
                g.DrawLine(gridPen, 0, i * CellSize, Board.Size * CellSize, i * CellSize);
            }

            using var font = new Font("Arial", 20, FontStyle.Bold);
            using var brushX = new SolidBrush(Color.Blue);
            using var brushO = new SolidBrush(Color.Red);

            // Vẽ X / O 
            for (int r = 0; r < Board.Size; r++)
            {
                for (int c = 0; c < Board.Size; c++)
                {
                    int cell = _board.GetCell(r, c);
                    if (cell == 0) continue;

                    string mark = (cell == 1) ? "X" : "O";
                    var x = c * CellSize + 6;
                    var y = r * CellSize + 4;
                    g.DrawString(mark, font, mark == "X" ? brushX : brushO, x, y);
                }
            }
        }

        public (int row, int col)? PointToCell(Point p)
        {
            int col = p.X / CellSize;
            int row = p.Y / CellSize;

            if (row >= 0 && row < Board.Size && col >= 0 && col < Board.Size)
                return (row, col);

            return null;
        }

        public void Refresh() => _canvas.Invalidate();
    }
}
