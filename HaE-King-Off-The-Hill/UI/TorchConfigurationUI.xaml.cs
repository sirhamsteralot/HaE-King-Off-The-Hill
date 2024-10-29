using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HaE_King_Off_The_Hill.UI
{
    /// <summary>
    /// Interaction logic for TorchConfigurationUC.xaml
    /// </summary>
    public partial class TorchConfigurationUI : UserControl
    {
        private KingOffTheHill kingOfTheHillPlugin = null;


        public TorchConfigurationUI(KingOffTheHill kingOfTheHillPlugin)
        {
            InitializeComponent();
            this.kingOfTheHillPlugin = kingOfTheHillPlugin;

            var configuration = kingOfTheHillPlugin.GetConfiguration();
            gridEntityId_tb.Text = configuration.ButtonGridEntityId.ToString();
            makeInvulnerable_cb.IsChecked = configuration.ButtonGridInvulnerable;
            buttonName_tb.Text = configuration.ButtonName;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!long.TryParse(gridEntityId_tb.Text, out long gridEntityId))
                return;

            string buttonNameCopy = String.Copy(buttonName_tb.Text);
            bool invulnerableCopy = makeInvulnerable_cb.IsChecked ?? false;

            kingOfTheHillPlugin.InvokeOnKOTHThread(() => {
                var options = new Configuration.KingOfTheHillConfig.Options();

                options.ButtonGridEntityId = gridEntityId;
                options.ButtonName = buttonNameCopy;
                options.ButtonGridInvulnerable = invulnerableCopy;

                kingOfTheHillPlugin.UpdateConfiguration(options);
            });
        }
    }
}
