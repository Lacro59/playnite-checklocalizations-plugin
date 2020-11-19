using CheckLocalizations.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Localization = CheckLocalizations.Models.Localization;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClDescriptionIntegration.xaml
    /// </summary>
    public partial class ClDescriptionIntegration : StackPanel
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private bool _withContener;
        private ClListViewLanguages PART_ListViewLanguages;


        public ClDescriptionIntegration(bool IntegrationShowTitle, bool withContener, List<Localization> gameLocalizations)
        {
            InitializeComponent();

            _withContener = withContener;

            PART_ListViewLanguages = new ClListViewLanguages(true);

            if (!IntegrationShowTitle)
            {
                PART_Title.Visibility = Visibility.Collapsed;
                PART_Separator.Visibility = Visibility.Collapsed;
                PART_ClList.Margin = new Thickness(0, 0, 0, 0);
            }

            PART_ClList.Children.Add(PART_ListViewLanguages);

            DataContext = this;
        }

        public void SetGameLocalizations(List<Localization> gameLocalizations)
        {
            PART_ListViewLanguages.SetGameLocalizations(gameLocalizations);
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
