using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Screens.DebugScreens.Game;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace HaE_King_Off_The_Hill
{
    public class InterferenceManager
    {
        public InterferenceManager() { }

        public void CreateInterference(Vector3D location, Vector3D direction, string prefabName)
        {
            MyVisualScriptLogicProvider.SpawnPrefabInGravity(prefabName, location, direction);
        }
    }
}
