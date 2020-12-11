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
using System.Windows.Data;
using System.Windows.Threading;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClListViewLanguages.xaml
    /// </summary>
    public partial class ClListViewLanguages : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private bool _WithColNotes;


        public ClListViewLanguages(bool WithColNotes)
        {
            _WithColNotes = WithColNotes;

            InitializeComponent();

            PART_ListViewLanguages.PreviewMouseWheel += Tools.HandlePreviewMouseWheel;

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
                        PART_ListViewLanguages.ItemsSource = PluginDatabase.GameSelectedData.Items;
                    }));
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        PART_ListViewLanguages.ItemsSource = null;
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }


        public void SetGameLocalizations()
        {
            PART_ListViewLanguages.ItemsSource = CheckLocalizations.PluginDatabase.GameSelectedData.Items;
        }


        private void PART_ListViewLanguages_Loaded(object sender, RoutedEventArgs e)
        {
            IntegrationUI.SetControlSize(PART_GridContener);

            if (_WithColNotes)
            {
#if DEBUG
                logger.Debug($"CheckLocalizations - PART_ListViewLanguages.ActualWidth: {PART_ListViewLanguages.ActualWidth}");
#endif
                if (!double.IsNaN(PART_ListViewLanguages.ActualWidth))
                {
                    double Width = PART_ListViewLanguages.ActualWidth - 150 - 70 - 70 - 70 - 30;

                    if (Width > 0)
                    {
                        PART_ColNotes.Width = Width;
                    }
                }
                else
                {
                    PART_ColNotes.Width = 100;
                }
            }
            else
            {
                PART_ColNotes.Width = 0;
            }
        }
    }
}
