using NLog.Fluent;
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
        public enum Tier
        {
            Start,
            Low,
            Medium,
            High,
            Extreme
        }

        public Dictionary<Tier, List<string>> TieredPrefabs { get; set; }

        private Random random = null;
        private long interferenceOwnerId;

        public InterferenceManager(long interferenceOwnerId) { 
            TieredPrefabs = new Dictionary<Tier, List<string>>();
            random = new Random();

            this.interferenceOwnerId = interferenceOwnerId;
        }

        public void RegisterPrefabAsTier(Tier tier, string prefabName)
        {
            if (!TieredPrefabs.ContainsKey(tier)) 
            { 
                TieredPrefabs[tier] = new List<string>();
            }

            TieredPrefabs[tier].Add(prefabName);
        }

        public void CreateInterference(Vector3D location, Vector3D direction, Tier tier)
        {
            if (TryGetRandomPrefab(tier, out string prefabName))
            {
                Log.Debug($"Creating interference with {prefabName}");
                MyVisualScriptLogicProvider.SpawnPrefabInGravity(prefabName, location, direction, interferenceOwnerId);
            }
        }

        private bool TryGetRandomPrefab(Tier tier, out string prefabName)
        {
            if (TieredPrefabs.TryGetValue(tier, out List<string> list))
            {
                if (list.Count > 0)
                {
                    prefabName = list[random.Next(0, list.Count - 1)];
                    return true;
                }
            }

            prefabName = "";
            return false;
        }
    }
}
