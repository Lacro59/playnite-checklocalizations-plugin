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

        private bool _WithColNotes;


        public ClListViewLanguages(bool WithColNotes)
        {
            _WithColNotes = WithColNotes;

            InitializeComponent();

            if (!_WithColNotes)
            {
                PART_ColNotes.Width = 0;
            }

            CheckLocalizations.PluginDatabase.PropertyChanged += OnPropertyChanged;
        }

        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    PART_ListViewLanguages.ItemsSource = null;
                    PART_ListViewLanguages.ItemsSource = CheckLocalizations.PluginDatabase.GameSelectedData.Items;
                }));
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }

        public void SetGameLocalizations()
        {
            PART_ListViewLanguages.ItemsSource = null;
            PART_ListViewLanguages.ItemsSource = CheckLocalizations.PluginDatabase.GameSelectedData.Items;
        }

        private void PART_ListViewLanguages_Loaded(object sender, RoutedEventArgs e)
        {
            // Define height & width
            var parent = ((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).Parent).Parent);

            if (!double.IsNaN(parent.ActualHeight))
            {
                PART_GridContener.Height = parent.ActualHeight;
            }

            if (!double.IsNaN(parent.Width))
            {
                PART_GridContener.Width = parent.ActualWidth;
            }

#if DEBUG
            logger.Debug($"CheckLocalizations - PART_ListViewLanguages_Loaded() - parent.name: {parent.Name} - parent.Height: {parent.Height} - parent.Width: {parent.Width}");
#endif

            if (!double.IsNaN(PART_ListViewLanguages.ActualWidth) && _WithColNotes)
            {
                double Width = PART_ListViewLanguages.ActualWidth - 150 - 70 - 70 - 70 - 30;

                if (Width > 0)
                {
                    PART_ColNotes.Width = Width;
                }
            }
        }
    }
}
