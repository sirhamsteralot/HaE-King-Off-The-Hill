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
            public int PeriodTimeS { get; set; }
            public int PercentagePerPeriod { get; set; }
            public int PointsPerCompletion { get; set; }
            public long ButtonGridEntityId { get; set; }
            public bool ButtonGridInvulnerable { get; set; }
            public string ButtonName { get; set; }

            public Options()
            {

            }

            public Options(Options clone)
            {
                this.PeriodTimeS = clone.PeriodTimeS;
                this.PercentagePerPeriod = clone.PercentagePerPeriod;
                this.ButtonGridEntityId = clone.ButtonGridEntityId;
                this.ButtonGridInvulnerable = clone.ButtonGridInvulnerable;
                this.ButtonName = String.Copy(clone.ButtonName);
            }

            public override string ToString()
            {
                return $"{PeriodTimeS}, {PercentagePerPeriod}, {ButtonGridEntityId}, {ButtonGridInvulnerable}, {ButtonName}";
            }
        }

        public List<PointCounter> Counters { get; set; }

        public Options Configuration { get; set; }
    }


}
