namespace Server.Game
{
    public static class CaroLogic
    {
        // thuat toan kiem tra thang thua
        public static bool CheckWin(int[,] board, int size, int row, int col, int player)
        {
            int[][] dirs = new int[][]
            {
                new[]{1,0}, new[]{0,1}, new[]{1,1}, new[]{1,-1}
            };

            foreach (var d in dirs)
            {
                int count = 1;
                count += Count(board, size, row, col, d[0], d[1], player);
                count += Count(board, size, row, col, -d[0], -d[1], player);
                if (count >= 5) return true;
            }
            return false;
        }

        private static int Count(int[,] b, int n, int r, int c, int dr, int dc, int p)
        {
            int k = 0; r += dr; c += dc;
            while (r >= 0 && c >= 0 && r < n && c < n && b[r, c] == p)
            {
                k++; r += dr; c += dc;
            }
            return k;
        }
    }
}
