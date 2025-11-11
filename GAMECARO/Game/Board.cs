namespace Client.Game
{
    public class Board
    {
        public const int Size = 15;
        private int[,] _grid = new int[Size, Size];
        public string NextTurn { get; private set; } = "X";
        public string? Winner { get; private set; }

    // ham nay dung de check va dat quan co
        public bool PlaceMark(int row, int col)
        {
            if (Winner != null) return false;
            if (_grid[row, col] != 0) return false;

            int mark = NextTurn == "X" ? 1 : 2;
            _grid[row, col] = mark;

            if (CheckWin(row, col, mark))
                Winner = NextTurn;
            else
                NextTurn = (NextTurn == "X") ? "O" : "X";

            return true;
        }

        // cập nhật toàn bộ bàn cờ từ dữ liệu server gửi về
        public void Update(int[][] board, string nextTurn, string? winner)
        {
            //  Cập nhật toàn bộ mảng
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    _grid[i, j] = board[i][j];

            //  Cập nhật lượt và người thắng
            NextTurn = nextTurn ?? "X";
            Winner = winner;
        }// done

        
        public int GetCell(int row, int col) => _grid[row, col];

        // checkwin
        private bool CheckWin(int r, int c, int player)
        {
            int[][] dirs =
            {
                new[]{1,0}, new[]{0,1}, new[]{1,1}, new[]{1,-1}
            };

            foreach (var d in dirs)
            {
                int count = 1;
                count += Count(r, c, d[0], d[1], player);
                count += Count(r, c, -d[0], -d[1], player);
                if (count >= 5) return true;
            }
            return false;
        }
        // dem so x , o lien tiep
        private int Count(int r, int c, int dr, int dc, int player)
        {
            int cnt = 0;
            while (true)
            {
                r += dr;
                c += dc;
                if (r < 0 || r >= Size || c < 0 || c >= Size) break;
                if (_grid[r, c] != player) break;
                cnt++;
            }
            return cnt;
        }
        public void Reset()
        {
            _grid = new int[Size, Size];
            NextTurn = "X";
            Winner = null;
        }

    }
}
