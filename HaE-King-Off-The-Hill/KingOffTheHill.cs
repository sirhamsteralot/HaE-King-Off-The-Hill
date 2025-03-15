using HaE_King_Off_The_Hill.Configuration;
using HaE_King_Off_The_Hill.UI;
using NLog;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

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
        private CancellationTokenSource _continueThread = new CancellationTokenSource();

        private Timer _scoreTimer = null;

        private long _king = 0;
        private IMyButtonPanel _hill = null;

        public KingOffTheHill() : base() { }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            _actionQueue = new BlockingCollection<Action>();
            _pluginThread = new Thread(() => {
                try
                {
                    foreach (var action in _actionQueue.GetConsumingEnumerable(_continueThread.Token))
                    {
                        action.Invoke();
                    }
                } catch (OperationCanceledException) { }
            });

            _pluginThread.Start();

            torch.GameStateChanged += Torch_GameStateChanged;

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

        private void Torch_GameStateChanged(MySandboxGame game, TorchGameState newState)
        {
            Log.Info($"new gamestate: {newState.ToString()}");

            switch (newState)
            {
                case TorchGameState.Loaded:
                    HookButton();
                    MySession.OnUnloading += MySession_OnUnloading;
                    MySession.OnSaved += KeenSession_OnSavingCheckpoint;
                    MyMultiplayer.Static.ClientJoined += Static_ClientJoined;
                    break;
            }
        }

        private void Static_ClientJoined(ulong arg1, string arg2)
        {
            long playerId = MyAPIGateway.Players.TryGetIdentityId(arg1);
            Scoreboard.EnableDisplay(playerId, false);
        }

        private void MySession_OnUnloading()
        {
            _scoreTimer.Dispose();
            _scoreTimer = null;

            SaveConfiguration();

            Log.Info("Cancelling plugin thread");

            _continueThread.CancelAfter(1000);
            _pluginThread.Join();

            Log.Info("Pluginthread rejoined");
        }

        private void KeenSession_OnSavingCheckpoint(bool success, string name)
        {
            SaveConfiguration();
        }

        public void SaveConfiguration()
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

        public void TimerCallback(object state) {
                InvokeOnKOTHThread(() =>
                {

                    if (_king != 0 && _configuration.Data.Configuration.ScoreCountingEnabled)
                    {
                        PointCounter counter = null;

                        if (!_pointCounters.TryGetValue(_king, out counter))
                        {
                            counter = new PointCounter(_king, 0);
                            _pointCounters.TryAdd(_king, counter);
                        }

                        counter.AddScore(_configuration.Data.Configuration.PointsPerPeriod);
                    }

                    UpdateScoreBoard();
                });
        }

        public void SetScoreCounting(bool IsScoreCountingEnabled)
        {
            InvokeOnKOTHThread(() =>
            {
                _configuration.Data.Configuration.ScoreCountingEnabled = IsScoreCountingEnabled;
                _configuration.Save();

                if (IsScoreCountingEnabled)
                {
                    Utilities.SendPlayerMessage(Torch, $"Score Counting Enabled! use !koth show to view scoreboard", 0ul, Color.Red);
                }
                else
                {
                    Utilities.SendPlayerMessage(Torch, $"Score Counting Disabled! use !koth hide to hide scoreboard", 0ul, Color.Red);
                }
            });
        }

        public void TakeControl(long factionId)
        {
            if (factionId == 0)
                return;

            if (!_configuration.Data.Configuration.ScoreCountingEnabled)
            {
                Log.Warn($"{factionId} Tried to take control, however score counting is disabled right now!");
                return;
            }

            Log.Info($"{factionId} Took Control!");

            _king = factionId;

            PointCounter counter = null;

            if (!_pointCounters.TryGetValue(_king, out counter))
            {
                counter = new PointCounter(_king, 0);
                _pointCounters.TryAdd(factionId, counter);
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

            SaveConfiguration();

            int periodTimeMs = _configuration.Data.Configuration.PeriodTimeS * 1000;
            _scoreTimer = new Timer(TimerCallback, this, periodTimeMs, periodTimeMs);

            HookButton();
        }

        public void HookButton()
        {
            Torch.Invoke(() => {

                if (_hill != null)
                {
                    _hill.ButtonPressed -= KingOffTheHill_ButtonPressed;
                }

                if (Torch.CurrentSession?.KeenSession != null)
                {
                    MyEntity entity = MyEntities.GetEntityById(_configuration.Data.Configuration.ButtonGridEntityId);

                    if (entity != null)
                    {
                        IMyCubeGrid cubegrid = entity as IMyCubeGrid;
                        if (cubegrid == null)
                        {
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
                    }
                    else
                    {
                        Log.Warn($"Failed to get cubegrid with ID: {_configuration.Data.Configuration.ButtonGridEntityId} !");
                    }
                }

            });

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

            string playerName = closest?.DisplayName ?? "error";
            Log.Info($"Button {buttonId} Pressed by {playerName}!");


            var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(closest.PlayerID);
            if (faction != null)
            {
                string factionTag = faction.Tag;
                if (faction.FactionId != _king)
                {
                    Utilities.SendPlayerMessage(Torch, $"{playerName} Took control for {factionTag}! use !koth show to view scoreboard", 0ul, Color.Red);
                }

                InvokeOnKOTHThread(() => {
                    TakeControl(faction.FactionId);
                });
            } else
            {
                Utilities.SendPlayerMessage(Torch, "You have to be in a faction to control the hill!", closest.SteamUserId, Color.Red);
            }
        }

        public KingOfTheHillConfig.Options GetConfiguration()
        {
             return new KingOfTheHillConfig.Options(_configuration.Data.Configuration);
        }
    }
}
