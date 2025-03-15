using NLog;
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
        private readonly Logger Log = LogManager.GetCurrentClassLogger();


        public TorchConfigurationUI(KingOffTheHill kingOfTheHillPlugin)
        {
            InitializeComponent();
            this.kingOfTheHillPlugin = kingOfTheHillPlugin;

            var configuration = kingOfTheHillPlugin.GetConfiguration();
            gridEntityId_tb.Text = configuration.ButtonGridEntityId.ToString();
            scoreCountingEnabled_cb.IsChecked = configuration.ScoreCountingEnabled;
            buttonName_tb.Text = configuration.ButtonName;
            pointsperperiod_tb.Text = configuration.PointsPerPeriod.ToString();
            periodtime_tb.Text = configuration.PeriodTimeS.ToString();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            bool parsingSuccess = true;

            if (!long.TryParse(gridEntityId_tb.Text, out long gridEntityId))
            {
                gridEntityId_tb.BorderBrush = Brushes.Red;
                parsingSuccess = false;
            } else
            {
                gridEntityId_tb.BorderBrush = Brushes.Transparent;
            }

            if (!int.TryParse(pointsperperiod_tb.Text, out int pointsPerPeriod))
            {
                pointsperperiod_tb.BorderBrush = Brushes.Red;
                parsingSuccess = false;
            } else
            {
                pointsperperiod_tb.BorderBrush = Brushes.Transparent;
            }

            if (!int.TryParse(periodtime_tb.Text, out int periodTimeS))
            {
                periodtime_tb.BorderBrush = Brushes.Red;
                parsingSuccess = false;
            } else
            {
                periodtime_tb.BorderBrush = Brushes.Transparent;
            }

            if (!parsingSuccess)
            {
                Log.Warn("Parsing config from torch UI Failed!");
                return;
            }

            string buttonNameCopy = String.Copy(buttonName_tb.Text);
            bool invulnerableCopy = scoreCountingEnabled_cb.IsChecked ?? false;

            kingOfTheHillPlugin.InvokeOnKOTHThread(() => {
                var options = new Configuration.KingOfTheHillConfig.Options();

                options.ButtonGridEntityId = gridEntityId;
                options.ButtonName = buttonNameCopy;
                options.ScoreCountingEnabled = invulnerableCopy;
                options.PointsPerPeriod = pointsPerPeriod;
                options.PeriodTimeS = periodTimeS;

                kingOfTheHillPlugin.UpdateConfiguration(options);
            });
        }
    }
}
