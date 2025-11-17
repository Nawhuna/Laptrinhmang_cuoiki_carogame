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

            // nếu có Winner mà chưa có WinningCells thì tự tìm 5 ô thắng
            EnsureWinningCellsComputed();

            // nền trắng
            g.Clear(Color.White);

            DrawBoard(g);
            DrawPieces(g);
            // KHÔNG vẽ thêm gạch đỏ, chỉ tô nền trong DrawPiece
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

        // ================== VẼ 1 QUÂN X / O (GIỮ NGUYÊN STYLE) ===================

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

            //  TÔ NỀN 5 Ô THẮNG
            if (isWinning)
            {
                // tô gần kín ô, chừa viền lưới 1px
                Rectangle bgRect = new Rectangle(
                    cellRect.X + 1,
                    cellRect.Y + 1,
                    cellRect.Width - 2,
                    cellRect.Height - 2);

                using (var backBrush = new SolidBrush(Color.FromArgb(255, 240, 100)))
                // vàng nhạt
                {
                    g.FillRectangle(backBrush, bgRect);
                }
            }

            // ===== PHẦN VẼ O – giữ nguyên =====
            if (mark == 'O' || mark == 'o')
            {
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
                // ===== PHẦN VẼ X – giữ nguyên =====
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

        // ================== TỰ TÌM 5 Ô THẮNG ===================

        private void EnsureWinningCellsComputed()
        {
            if (_board.Winner == null)
            {
                // chưa có ai thắng => không tô ô nào
                WinningCells = null;
                return;
            }

            if (WinningCells != null) return;

            int player = _board.Winner == "X" ? 1 : 2;

            // 4 hướng: ngang, dọc, chéo xuống, chéo lên
            int[,] dirs = new int[,]
            {
                { 0, 1 },   // ngang
                { 1, 0 },   // dọc
                { 1, 1 },   // chéo xuống
                { 1,-1 }    // chéo lên
            };

            for (int r = 0; r < Board.Size; r++)
            {
                for (int c = 0; c < Board.Size; c++)
                {
                    if (_board.GetCell(r, c) != player) continue;

                    for (int d = 0; d < 4; d++)
                    {
                        int dr = dirs[d, 0];
                        int dc = dirs[d, 1];

                        // bỏ qua nếu phía trước (ngược hướng) cũng là quân mình
                        int prevR = r - dr;
                        int prevC = c - dc;
                        if (prevR >= 0 && prevR < Board.Size &&
                            prevC >= 0 && prevC < Board.Size &&
                            _board.GetCell(prevR, prevC) == player)
                        {
                            continue;
                        }

                        var cells = new List<Point>();
                        int rr = r;
                        int cc = c;

                        while (rr >= 0 && rr < Board.Size &&
                               cc >= 0 && cc < Board.Size &&
                               _board.GetCell(rr, cc) == player)
                        {
                            cells.Add(new Point(cc, rr)); // X = col, Y = row
                            rr += dr;
                            cc += dc;
                        }

                        if (cells.Count >= 5)
                        {
                            WinningCells = cells.GetRange(0, 5); // lấy 5 ô đầu tiên
                            return;
                        }
                    }
                }
            }
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
