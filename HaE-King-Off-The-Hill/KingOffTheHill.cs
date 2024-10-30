using HaE_King_Off_The_Hill.Configuration;
using HaE_King_Off_The_Hill.UI;
using NLog;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
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
using VRage.Game.ModAPI;
using VRage.ModAPI;

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
        private IMyButtonPanel _hill = null;

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

            torch.SessionUnloading += Torch_SessionUnloading;
            torch.SessionLoaded += Torch_SessionLoaded;

            _configuration = Persistent<KingOfTheHillConfig>.Load(Path.Combine(StoragePath, Name + ".cfg"));
            _pointCounters = new ConcurrentDictionary<long, PointCounter>();

            if (_configuration.Data.Counters == null)
                _configuration.Data.Counters = new List<PointCounter>();
            if (_configuration.Data.Configuration == null)
                _configuration.Data.Configuration = new KingOfTheHillConfig.Options();

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

                UpdateScoreBoard();
            });
        }

        private void Counter_UplinkComplete(long obj)
        {
            InvokeOnKOTHThread(() => {
                if (_pointCounters.TryGetValue(obj, out PointCounter counter))
                {
                    counter.AddScore(_configuration.Data.Configuration.PointsPerCompletion);
                }
            });
        }

        public void TakeControl(long factionId)
        {
            if (factionId == 0)
                return;

            Log.Info($"{factionId} Took Control!");

            _king = factionId;

            PointCounter counter = null;

            if (!_pointCounters.TryGetValue(_king, out counter))
            {
                counter = new PointCounter(_king, 0);
                _pointCounters.TryAdd(factionId, counter);
                counter.UplinkComplete += Counter_UplinkComplete;
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

        public void Torch_SessionUnloading()
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
        private void Torch_SessionLoaded()
        {
            HookButton();
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

            Torch_SessionUnloading();

            int periodTimeMs = _configuration.Data.Configuration.PeriodTimeS * 1000;
            _scoreTimer = new Timer(TimerCallback, this, periodTimeMs, periodTimeMs);

            HookButton();
        }

        public void HookButton()
        {
            if (_hill != null)
            {
                _hill.ButtonPressed -= KingOffTheHill_ButtonPressed;
            }

            if (Torch.CurrentSession?.KeenSession != null)
            {
                IMyEntity entity = MyAPIGateway.Entities.GetEntity(x => x.EntityId == _configuration.Data.Configuration.ButtonGridEntityId);

                if (entity != null)
                {
                    IMyCubeGrid cubegrid = entity as IMyCubeGrid;
                    if (cubegrid == null) {
                        Log.Warn("failed to cast entity to cubegrid!");
                        return;
                    }

                    Log.Info($"cubegrid {cubegrid.CustomName} found!");

                    List<IMySlimBlock> slimblocks = new List<IMySlimBlock>();

                    cubegrid.GetBlocks(slimblocks, x => { return x.FatBlock is IMyButtonPanel && ((IMyButtonPanel)x.FatBlock).CustomName == _configuration.Data.Configuration.ButtonName; });

                    Log.Info($"{slimblocks.Count()} Found on {cubegrid.CustomName}, hooking to 1st...");

                    if (slimblocks.Count > 0)
                    {
                        _hill = slimblocks.First().FatBlock as IMyButtonPanel;
                        _hill.ButtonPressed += KingOffTheHill_ButtonPressed;
                        Log.Info($"{_hill.CustomName} Hooked!");
                    }
                } else
                {
                    Log.Warn($"Failed to get cubegrid with ID: {_configuration.Data.Configuration.ButtonGridEntityId} !");
                }
            }
        }

        private void KingOffTheHill_ButtonPressed(int buttonId)
        {

            IMyPlayer closest = null;
            double closestSq = double.MaxValue;
            if (_hill == null)
            {
                Log.Warn("No Hill registered! why did this trigger?!");
                return;
            }

            MyAPIGateway.Players.GetPlayers(new List<IMyPlayer>(), x => {
                double distanceSq = (x.GetPosition() - _hill.GetPosition()).LengthSquared();
                if (distanceSq < closestSq)
                {
                    closest = x;
                    closestSq = distanceSq;
                }

                return false;
            });

            Log.Info($"Button {buttonId} Pressed by {closest?.DisplayName ?? "error"}!");

            var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(closest.PlayerID);
            if (faction != null)
            {
                InvokeOnKOTHThread(() => {
                    TakeControl(faction.FactionId);
                });
            }
        }

        public KingOfTheHillConfig.Options GetConfiguration()
        {
             return new KingOfTheHillConfig.Options(_configuration.Data.Configuration);
        }
    }
}
