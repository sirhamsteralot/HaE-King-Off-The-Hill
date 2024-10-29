using HaE_King_Off_The_Hill.Configuration;
using HaE_King_Off_The_Hill.UI;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;

namespace HaE_King_Off_The_Hill
{
    public class KingOffTheHill : TorchPluginBase, IWpfPlugin
    {
        public ClientScoreboard Scoreboard { get; set; } = null;

        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        private TorchConfigurationUI _configurationUI = null;
        private Persistent<KingOfTheHillConfig> _configuration = null;
        private Dictionary<long, PointCounter> _pointCounters = null;


        public KingOffTheHill() : base() { }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            _pointCounters = new Dictionary<long, PointCounter>();
            _configuration = Persistent<KingOfTheHillConfig>.Load(Path.Combine(StoragePath, Name + ".cfg"));

            Scoreboard = new ClientScoreboard();

            // Test Code;
            AddScore(10, 999);
            AddScore(2, 999);
            AddScore(101231231, 999);
        }

        public List<PointCounter> GetCurrentScore()
        {
            return _pointCounters.Values.ToList();
        }

        public void SaveConfig()
        {
            if (_configuration != null)
            {
                _configuration.Data.Counters.Clear();

                foreach (var counter in _pointCounters.Values.ToList())
                {
                    _configuration.Data.Counters.Add(counter);
                }

                _configuration.Save();
            }
        }

        public void AddScore(long factionId, int points)
        {
            if (_pointCounters.TryGetValue(factionId, out PointCounter score))
            {
                score.Points += points;
            }
            else
            {
                _pointCounters.Add(factionId, new PointCounter(factionId, points));
            }
        }

        public UserControl GetControl()
        {
            if (_configurationUI == null)
            {
                _configurationUI = new TorchConfigurationUI();
            }

            return _configurationUI;
        }

        public void UpdateScoreBoard()
        {
            Scoreboard.UpdateDisplay(_pointCounters.Values.ToList());
        }
    }
}
