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
        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        private TorchConfigurationUI _configurationUI = null;
        private Persistent<KingOfTheHillConfig> _configuration = null;

        public KingOffTheHill() : base() { }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            _configuration = Persistent<KingOfTheHillConfig>.Load(Path.Combine(StoragePath, Name, ".cfg"));
        }

        public UserControl GetControl()
        {
            if (_configurationUI == null)
            {
                _configurationUI = new TorchConfigurationUI();
            }

            return _configurationUI;
        }
    }
}
