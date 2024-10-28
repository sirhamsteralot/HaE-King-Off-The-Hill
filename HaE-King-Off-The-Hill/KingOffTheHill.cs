using HaE_King_Off_The_Hill.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Torch;
using Torch.API.Plugins;

namespace HaE_King_Off_The_Hill
{
    public class KingOffTheHill : TorchPluginBase, IWpfPlugin
    {
        private TorchConfigurationUC _torchConfiguration = null;
        public UserControl GetControl()
        {
            if (_torchConfiguration == null)
            {
                _torchConfiguration = new TorchConfigurationUC();
            }

            return _torchConfiguration;
        }
    }
}
