using CheckLocalizations.Models;
using Playnite.Controls;
using PluginCommon;
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

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : WindowBase
    {
        public CheckLocalizationsView(List<GameLocalization> gameLocalizations)
        {
            InitializeComponent();
            ListViewLanguages.ItemsSource = gameLocalizations;
            DataContext = this;
        }

        private void ListViewLanguages_Loaded(object sender, RoutedEventArgs e)
        {
            Tools.DesactivePlayniteWindowControl(this);
        }
    }
}
