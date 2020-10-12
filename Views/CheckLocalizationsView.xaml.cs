using CheckLocalizations.Models;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.API;
using PluginCommon.PlayniteResources.Common;
using PluginCommon.PlayniteResources.Converters;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : UserControl
    {
        public CheckLocalizationsView(List<GameLocalization> gameLocalizations)
        {
            InitializeComponent();

            ListViewLanguages.ItemsSource = gameLocalizations;

            DataContext = this;
        }
    }
}
