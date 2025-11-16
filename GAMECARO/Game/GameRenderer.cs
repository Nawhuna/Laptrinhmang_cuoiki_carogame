using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Client.Game
{
    public class GameRenderer
    {
        private readonly Panel _canvas;
        private readonly Board _board;
        private const int CellSize = 35;

        // Point.X = col, Point.Y = row
        public List<Point>? WinningCells { get; set; }

        public GameRenderer(Panel canvas, Board board)
        {
            _canvas = canvas;
            _board = board;
            _canvas.Paint += Canvas_Paint;
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // nền trắng
            g.Clear(Color.White);

            DrawBoard(g);
            DrawPieces(g);
            DrawWinningLine(g);
        }

        // ================== VẼ LƯỚI ===================

        private void DrawBoard(Graphics g)
        {
            using var pen = new Pen(Color.FromArgb(210, 210, 210), 1f);

            int sizePx = Board.Size * CellSize;

            for (int i = 0; i <= Board.Size; i++)
            {
                int pos = i * CellSize;
                g.DrawLine(pen, pos, 0, pos, sizePx);   // dọc
                g.DrawLine(pen, 0, pos, sizePx, pos);   // ngang
            }
        }

        // ================== VẼ TOÀN BỘ QUÂN CỜ ===================

        private void DrawPieces(Graphics g)
        {
            for (int r = 0; r < Board.Size; r++)
            {
                for (int c = 0; c < Board.Size; c++)
                {
                    int cell = _board.GetCell(r, c); // 0 = trống, 1 = X, 2 = O
                    if (cell == 0) continue;

                    char mark = (cell == 1) ? 'X' : 'O';

                    bool isWinning = false;
                    if (WinningCells != null)
                    {
                        foreach (var p in WinningCells)
                        {
                            if (p.X == c && p.Y == r)
                            {
                                isWinning = true;
                                break;
                            }
                        }
                    }

                    DrawPiece(g, r, c, mark, isWinning);
                }
            }
        }

        // ================== VẼ 1 QUÂN X / O ===================

        private void DrawPiece(Graphics g, int row, int col, char mark, bool isWinning)
        {
            Rectangle cellRect = new Rectangle(
                col * CellSize,
                row * CellSize,
                CellSize,
                CellSize);

            // vùng quân cờ (hơi nhỏ hơn ô)
            Rectangle pieceRect = new Rectangle(
                cellRect.X + 6,
                cellRect.Y + 6,
                CellSize - 12,
                CellSize - 12);

            if (mark == 'O' || mark == 'o')
            {
                // ===== O mảnh, giống độ dày X =====
                Color main = Color.FromArgb(0, 200, 0);     // xanh lá
                Color shadow = Color.FromArgb(70, 0, 0, 0); // bóng mờ

                // bóng lệch nhẹ
                Rectangle shadowCircle = new Rectangle(
                    pieceRect.X + 1,
                    pieceRect.Y + 1,
                    pieceRect.Width,
                    pieceRect.Height);

                using (var shadowPen = new Pen(shadow, 5)
                {
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    g.DrawEllipse(shadowPen, shadowCircle);
                }

                using (var penO = new Pen(main, 4)
                {
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    g.DrawEllipse(penO, pieceRect);
                }
            }
            else
            {
                // ===== X màu đỏ + bóng mờ =====
                Color mainColor = Color.Red;
                Color shadowColor = Color.FromArgb(120, 0, 0, 0);

                Point p1 = new Point(pieceRect.Left, pieceRect.Top);
                Point p2 = new Point(pieceRect.Right, pieceRect.Bottom);
                Point p3 = new Point(pieceRect.Right, pieceRect.Top);
                Point p4 = new Point(pieceRect.Left, pieceRect.Bottom);

                // bóng lệch nhẹ xuống phải
                Point sp1 = new Point(p1.X + 1, p1.Y + 1);
                Point sp2 = new Point(p2.X + 1, p2.Y + 1);
                Point sp3 = new Point(p3.X + 1, p3.Y + 1);
                Point sp4 = new Point(p4.X + 1, p4.Y + 1);

                using (var shadowPen = new Pen(shadowColor, 6)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                })
                {
                    g.DrawLine(shadowPen, sp1, sp2);
                    g.DrawLine(shadowPen, sp3, sp4);
                }

                using (var mainPen = new Pen(mainColor, 4)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                })
                {
                    g.DrawLine(mainPen, p1, p2);
                    g.DrawLine(mainPen, p3, p4);
                }
            }
        }

        // ================== GẠCH ĐỎ 5 Ô THẮNG ===================

        private void DrawWinningLine(Graphics g)
        {
            if (WinningCells == null || WinningCells.Count < 2)
                return;

            Point first = WinningCells[0];
            Point last = WinningCells[WinningCells.Count - 1];

            Point p1 = GetCellCenter(first.Y, first.X); // row, col
            Point p2 = GetCellCenter(last.Y, last.X);

            using var pen = new Pen(Color.Red, 3)
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            g.DrawLine(pen, p1, p2);
        }

        private static Point GetCellCenter(int row, int col)
        {
            int x = col * CellSize + CellSize / 2;
            int y = row * CellSize + CellSize / 2;
            return new Point(x, y);
        }

        // Chuyển tọa độ chuột -> (row, col)
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
