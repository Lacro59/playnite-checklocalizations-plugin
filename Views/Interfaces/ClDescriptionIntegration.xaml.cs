using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonShared;
using Newtonsoft.Json;
using Playnite.SDK;
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
                if (e.PropertyName == "GameSelectedData" || e.PropertyName == "PluginSettings")
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        if (PluginDatabase.PluginSettings.IntegrationShowTitle)
                        {
                            PART_ClList.Margin = new Thickness(0, 5, 0, 5);
                        }

                        this.DataContext = new
                        {
                            IntegrationShowTitle = PluginDatabase.PluginSettings.IntegrationShowTitle
                        };
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }
    }
}
