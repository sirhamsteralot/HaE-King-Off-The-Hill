﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaE_King_Off_The_Hill.Configuration
{
    public class PointCounter
    {
        public long FactionId { get; set; }
        public int Points { get; set; }

        public PointCounter() { }

        public PointCounter(long factionId, int points)
        {
            this.FactionId = factionId;
            this.Points = points;
        }
    }
}