using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaE_King_Off_The_Hill.Configuration
{
    public class KingOfTheHillConfig
    {
        public int PointsPerHour { get; set; }

        public List<PointCounter> Counters { get; set; }
    }
}
