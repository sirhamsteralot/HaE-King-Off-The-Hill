using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game.ModAPI;

namespace HaE_King_Off_The_Hill.Commands
{
    [Category("koth")]
    public class PlayerCommands : CommandModule
    {
        [Command("score", "Shows the current scores for all factions who have scored")]
        [Permission(MyPromoteLevel.None)]
        public void Score()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;
            var sb = new StringBuilder();

            foreach (var counter in kothPlugin.GetCurrentScore())
            {
                sb.Append(counter.FactionId.ToString()).Append(" | ").AppendLine(counter.Points.ToString());
            }

            ModCommunication.SendMessageTo(new DialogMessage("Points", null, sb.ToString()), Context.Player.SteamUserId);
        }

        [Command("show", "enables showing scoreboard for player")]
        [Permission(MyPromoteLevel.None)]
        public void Show()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;
            kothPlugin.Scoreboard.EnableDisplay(Context.Player.IdentityId, true);
            Context.Respond("Scoreboard Shown (it will show up when the points get updated)");
        }

        [Command("hide", "disables showing scoreboard for player")]
        [Permission(MyPromoteLevel.None)]
        public void Hide()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;
            kothPlugin.Scoreboard.EnableDisplay(Context.Player.IdentityId, false);
            Context.Respond("Scoreboard Hidden");
        }

        [Command("enable", "enables/disables score counting")]
        [Permission(MyPromoteLevel.Admin)]
        public void Enable()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;
            kothPlugin.SetScoreCounting(true);
        }

        [Command("disable", "enables/disables score counting")]
        [Permission(MyPromoteLevel.Admin)]
        public void Disable()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;
            kothPlugin.SetScoreCounting(false);
        }
    }
}
