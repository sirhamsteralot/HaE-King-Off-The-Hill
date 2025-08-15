using HaE_King_Off_The_Hill.Configuration;
using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaE_King_Off_The_Hill.UI
{
    public class ClientScoreboard
    {
        public HashSet<long> EnabledPlayers { get; set; } = new HashSet<long>();
        public string KingTag { get; set; } = "None";

        public ClientScoreboard() { }

        public void CheckOnJoin(long player)
        {
            if (!EnabledPlayers.Contains(player))
                EnableDisplay(player, false);
        }

        public void EnableDisplay(long player, bool enabled = true)
        {
            if (enabled)
            {
                EnabledPlayers.Add(player);
            }
            else
            {
                EnabledPlayers.Remove(player);
                MyVisualScriptLogicProvider.SetQuestlogVisible(false, player);
            }
        }

        public void UpdateDisplay(List<PointCounter> pointCounters)
        {
            foreach (var playerId in EnabledPlayers)
            {
                MyVisualScriptLogicProvider.SetQuestlogVisible(true, playerId);
                MyVisualScriptLogicProvider.SetQuestlogTitle($"KOTH Score [{KingTag}]", playerId);
                MyVisualScriptLogicProvider.RemoveQuestlogDetails(playerId);

                var orderedList = pointCounters.OrderByDescending(x => x.Points);

                foreach (var pointCounter in orderedList)
                {
                    if (pointCounter.FactionId != 0)
                        MyVisualScriptLogicProvider.AddQuestlogDetail(pointCounter.ToString(), false, false, playerId);
                }
            }
        }
    }
}
