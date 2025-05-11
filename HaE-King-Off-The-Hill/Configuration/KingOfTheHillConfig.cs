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
            public int PointsDeductedOnDeath { get; set; } = 10;
            public long ButtonGridEntityId { get; set; }
            public bool ScoreCountingEnabled { get; set; } = true;
            public string ButtonName { get; set; } = "";

            public Options()
            {
            }

            public Options(Options clone)
            {
                this.PeriodTimeS = clone.PeriodTimeS;
                this.PointsPerPeriod = clone.PointsPerPeriod;
                this.PointsDeductedOnDeath = clone.PointsDeductedOnDeath;
                this.ButtonGridEntityId = clone.ButtonGridEntityId;
                this.ScoreCountingEnabled = clone.ScoreCountingEnabled;
                this.ButtonName = String.Copy(clone.ButtonName);
            }

            public override string ToString()
            {
                return $"{PeriodTimeS}, {PointsPerPeriod}, {PointsDeductedOnDeath}, {ButtonGridEntityId}, {ButtonName}, {ScoreCountingEnabled}";
            }
        }

        public List<PointCounter> Counters { get; set; }

        public Options Configuration { get; set; }
    }


}
