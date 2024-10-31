using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaE_King_Off_The_Hill.Configuration
{
    public class KingOfTheHillConfig
    {
        public class Options
        {
            public int PeriodTimeS { get; set; } = 10;
            public int PointsPerPeriod { get; set; } = 1;
            public long ButtonGridEntityId { get; set; }
            public bool ButtonGridInvulnerable { get; set; }
            public string ButtonName { get; set; } = "";

            public Options()
            {
            }

            public Options(Options clone)
            {
                this.PeriodTimeS = clone.PeriodTimeS;
                this.PointsPerPeriod = clone.PointsPerPeriod;
                this.ButtonGridEntityId = clone.ButtonGridEntityId;
                this.ButtonGridInvulnerable = clone.ButtonGridInvulnerable;
                this.ButtonName = String.Copy(clone.ButtonName);
            }

            public override string ToString()
            {
                return $"{PeriodTimeS}, {PointsPerPeriod}, {ButtonGridEntityId}, {ButtonGridInvulnerable}, {ButtonName}";
            }
        }

        public List<PointCounter> Counters { get; set; }

        public Options Configuration { get; set; }
    }


}
