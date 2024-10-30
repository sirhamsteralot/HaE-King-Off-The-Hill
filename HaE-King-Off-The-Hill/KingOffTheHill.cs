using HaE_King_Off_The_Hill.Configuration;
using HaE_King_Off_The_Hill.UI;
using NLog;
using Sandbox.Engine.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private ConcurrentDictionary<long, PointCounter> _pointCounters = null;

        private BlockingCollection<Action> _actionQueue = null;
        private Thread _pluginThread = null;

        private Timer _scoreTimer = null;

        private long _king = 0;

        public KingOffTheHill() : base() { }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            _actionQueue = new BlockingCollection<Action>();
            _pluginThread = new Thread(() => {
                foreach (var action in _actionQueue.GetConsumingEnumerable())
                {
                    action.Invoke();
                }
            });

            _pluginThread.Start();

            torch.SessionUnloading += SaveConfig;

            _configuration = Persistent<KingOfTheHillConfig>.Load(Path.Combine(StoragePath, Name + ".cfg"));
            _pointCounters = new ConcurrentDictionary<long, PointCounter>();
            foreach (var counter in _configuration.Data.Counters)
            {
                _pointCounters.TryAdd(counter.FactionId, new PointCounter(counter));
            }

            Scoreboard = new ClientScoreboard();

            int periodTimeMs = _configuration.Data.Configuration.PeriodTimeS * 1000;
            _scoreTimer = new Timer(TimerCallback, this, periodTimeMs, periodTimeMs);
        }

        public void TimerCallback(object state) {
            InvokeOnKOTHThread(() => { 
                if (_king != 0)
                {
                    PointCounter counter = null;

                    if (!_pointCounters.TryGetValue(_king, out counter))
                    {
                        counter = new PointCounter(_king, 0);
                        _pointCounters.TryAdd(_king, counter);
                    }

                    counter.AddPercentage(_configuration.Data.Configuration.PercentagePerPeriod);
                }
            });
        }

        public void UplinkCompletedCallback(long factionId)
        {
            if (_pointCounters.TryGetValue(factionId, out var counter))
            {
                counter.AddScore(_configuration.Data.Configuration.PointsPerCompletion);
            }
        }

        public void InvokeOnKOTHThread(Action action)
        {
            _actionQueue.Add(action);
        }

        public List<PointCounter> GetCurrentScore()
        {
            return _pointCounters.Values.ToList();
        }

        public void SaveConfig()
        {
            InvokeOnKOTHThread(() => {
                if (_configuration != null)
                {
                    _configuration.Data.Counters.Clear();

                    foreach (var counter in _pointCounters.Values.ToList())
                    {
                        _configuration.Data.Counters.Add(counter);
                    }
                    _configuration.Save();

                    Log.Info($"Saved configuration");
                }
            });

        }

        public void AddScore(long factionId, int points)
        {
            if (_pointCounters.TryGetValue(factionId, out PointCounter score))
            {
                score.Points += points;
            }
            else
            {
                _pointCounters.TryAdd(factionId, new PointCounter(factionId, points));
            }
        }

        public UserControl GetControl()
        {
            if (_configurationUI == null)
            {
                _configurationUI = new TorchConfigurationUI(this);
            }

            return _configurationUI;
        }

        public void UpdateScoreBoard()
        {
            Scoreboard.UpdateDisplay(_pointCounters.Values.ToList());
        }

        /// <summary>
        /// Must be invoked from koth thread using InvokeOnKOTHThread(x)
        /// </summary>
        /// <param name="config"></param>
        public void UpdateConfiguration(KingOfTheHillConfig.Options config)
        {
            _configuration.Data.Configuration = config;

            Log.Info($"Updated configuration: {config.ToString()}");

            SaveConfig();

            int periodTimeMs = _configuration.Data.Configuration.PeriodTimeS * 1000;
            _scoreTimer = new Timer(TimerCallback, this, periodTimeMs, periodTimeMs);
        }

        public KingOfTheHillConfig.Options GetConfiguration()
        {
             return new KingOfTheHillConfig.Options(_configuration.Data.Configuration);
        }
    }
}
