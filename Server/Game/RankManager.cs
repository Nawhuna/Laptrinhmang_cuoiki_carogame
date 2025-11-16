using Server.Model;
namespace Server.Game

{

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

        public static List<RankInfo> GetTopPlayers(int count = 10)
        {
            return _ranks.Values
                .OrderByDescending(r => r.Score)
                .Take(count)
                .ToList();
        }
    }
}
