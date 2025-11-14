namespace Server.Game
{
    public class RankInfo
    {
        public string PlayerId { get; set; } = "";
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Score { get; set; } = 1000; 
    }

    public static class RankManager
    {
        private static readonly Dictionary<string, RankInfo> _ranks = new();

        public static RankInfo Get(string playerId)
        {
            if (!_ranks.ContainsKey(playerId))
                _ranks[playerId] = new RankInfo { PlayerId = playerId };
            return _ranks[playerId];
        }

        public static void AddWin(string playerId)
        {
            var info = Get(playerId);
            info.Wins++;
            info.Score += 15;
        }

        public static void AddLoss(string playerId)
        {
            var info = Get(playerId);
            info.Losses++;
            info.Score -= 10;
            if (info.Score < 0) info.Score = 0;
        }
    }
}
