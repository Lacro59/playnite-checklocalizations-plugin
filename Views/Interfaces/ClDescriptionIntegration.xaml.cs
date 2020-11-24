using CheckLocalizations.Models;
using CheckLocalizations.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Localization = CheckLocalizations.Models.Localization;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClDescriptionIntegration.xaml
    /// </summary>
    public partial class ClDescriptionIntegration : StackPanel
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private bool _withContener;
        private ClListViewLanguages PART_ListViewLanguages;


        public ClDescriptionIntegration(bool withContener)
        {
            InitializeComponent();

            _withContener = withContener;

            PART_ListViewLanguages = new ClListViewLanguages(true);

            PART_ClList.Children.Add(PART_ListViewLanguages);

            PluginDatabase.PropertyChanged += OnPropertyChanged;
        }


        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
#if DEBUG
                logger.Debug($"ClDescriptionIntegration.OnPropertyChanged({e.PropertyName}): {JsonConvert.SerializeObject(PluginDatabase.GameSelectedData)}");
#endif
                if (e.PropertyName == "GameSelectedData" || e.PropertyName == "PluginSettings")
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        if (PluginDatabase.PluginSettings.IntegrationShowTitle && !PluginDatabase.PluginSettings.EnableIntegrationInCustomTheme)
                        {
                            PART_Title.Visibility = Visibility.Visible;
                            PART_Separator.Visibility = Visibility.Visible;
                            PART_ClList.Margin = new Thickness(0, 5, 0, 5);
                        }
                        else
                        {
                            PART_Title.Visibility = Visibility.Collapsed;
                            PART_Separator.Visibility = Visibility.Collapsed;
                            PART_ClList.Margin = new Thickness(0, 0, 0, 0);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }


        private void PART_ClList_Loaded(object sender, RoutedEventArgs e)
        {
            // Define height & width
            var parent = ((FrameworkElement)((FrameworkElement)sender).Parent);
            if (_withContener)
            {
                parent = ((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
            }

#if DEBUG
            logger.Debug($"CheckLocalizations - PART_ClList_Loaded({_withContener}) - parent.name: {parent.Name} - parent.Height: {parent.Height} - parent.Width: {parent.Width}");
#endif

            if (!double.IsNaN(parent.Height))
            {
                PART_ListViewLanguages.Height = parent.Height;
            }

            if (!double.IsNaN(parent.Width))
            {
                PART_ListViewLanguages.Width = parent.Width;
            }
        }
    }
}
