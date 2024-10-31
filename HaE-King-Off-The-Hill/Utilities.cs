using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.API;
using Torch.API.Managers;
using Torch.Utils;
using VRage.Scripting;
using VRageMath;

namespace HaE_King_Off_The_Hill
{
    static internal class Utilities
    {
        public static void SendPlayerMessage(ITorchBase torch, string message, ulong targetSteamId, Color color = default(Color), string font = null)
        {
            IChatManagerServer manager = torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
            if (color == default(Color) && font != null)
            {
                color = ColorUtils.TranslateColor(font);
            }

            if (font == null)
            {
                font = "White";
            }

            manager?.SendMessageAsOther("KOTH", message, color, targetSteamId, font);
        }
    }
}
