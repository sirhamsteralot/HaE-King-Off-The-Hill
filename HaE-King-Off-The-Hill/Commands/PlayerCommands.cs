using Sandbox.Definitions;
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
        }

        [Command("hide", "disables showing scoreboard for player")]
        [Permission(MyPromoteLevel.None)]
        public void Hide()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;
            kothPlugin.Scoreboard.EnableDisplay(Context.Player.IdentityId, false);
        }

        [Command("spawnattack", "spawns a factorum encounter at the hill")]
        [Permission(MyPromoteLevel.Admin)]
        public void SpawnAttack()
        {
            var kothPlugin = Context.Plugin as KingOffTheHill;


            try
            {
                if (Context.Args.Count > 0)
                {
                    kothPlugin.InterferenceManager.CreateInterference(Context.Player.GetPosition() + Context.Player.Character.WorldMatrix.Up * 100, Context.Player.Character.WorldMatrix.Forward, Context.Args[0]);
                    Context.Respond("interference spawned!");
                }
            }
            catch (Exception ex)
            {
                Context.Respond(ex.Message);
            }
        }

        [Command("getprefabs", "gets all possible prefabs")]
        [Permission(MyPromoteLevel.Admin)]
        public void GetPrefabs()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var prefab in MyDefinitionManager.Static.GetPrefabDefinitions())
            {
                stringBuilder.AppendLine(prefab.Key + " : " + prefab.Value.PrefabPath);
            }

            ModCommunication.SendMessageTo(new DialogMessage("Prefabs", null, stringBuilder.ToString()), Context.Player.SteamUserId);
        }
    }
}
