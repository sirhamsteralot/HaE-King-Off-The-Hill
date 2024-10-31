using NLog.Fluent;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace HaE_King_Off_The_Hill.Configuration
{
    public class PointCounter
    {
        public long FactionId { get; set; }
        public int Points { get; set; }

        public PointCounter() { }

        public PointCounter(PointCounter clone)
        {
            FactionId = clone.FactionId;
            Points = clone.Points;
        }

        public PointCounter(long factionId, int points)
        {
            this.FactionId = factionId;
            this.Points = points;
        }

        public void AddScore(int score)
        {
            Points += score;
        }

        public override string ToString()
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(FactionId);
            string factionTag = "";

            if (faction != null)
            {
                Log.Info($"Getting faction by id {FactionId} failed..");
                factionTag = faction.Tag;
            } else
            {
                factionTag = FactionId.ToString();
            }

            return $"{factionTag}: {Points} pts";
        }
    }
}
