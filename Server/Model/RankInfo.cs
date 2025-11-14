using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    public class RankInfo
    {
        public string PlayerId { get; set; } = "";
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Score { get; set; } = 1000;
    }
}
