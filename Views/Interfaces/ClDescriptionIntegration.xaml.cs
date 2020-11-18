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

        public ClDescriptionIntegration(bool IntegrationShowTitle, bool withContener, List<Localization> gameLocalizations)
        {
            InitializeComponent();

            _withContener = withContener;

            if (!IntegrationShowTitle)
            {
                PART_Title.Visibility = Visibility.Collapsed;
                PART_Separator.Visibility = Visibility.Collapsed;
                PART_ClList.Margin = new Thickness(0, 0, 0, 0);
            }


            PART_ListViewLanguages.DataContext = gameLocalizations;
            
            DataContext = this;
        }

        public void SetGameLocalizations(List<Localization> gameLocalizations)
        {
            PART_ListViewLanguages.ItemsSource = null;
            gameLocalizations.Sort((x, y) => x.Language.CompareTo(y.Language));
            PART_ListViewLanguages.ItemsSource = gameLocalizations;
        }

        private void PART_ListViewLanguages_Loaded(object sender, RoutedEventArgs e)
        {
            // Define height & width
            var parent = ((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
            if (_withContener)
            {
                parent = ((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).Parent).Parent);
            }

#if DEBUG
            logger.Debug($"SuccessStory - CheckLocalizationsList({_withContener}) - parent.name: {parent.Name} - parent.Height: {parent.Height} - parent.Width: {parent.Width}");
#endif

            if (!double.IsNaN(parent.Height))
            {
                ((FrameworkElement)sender).Height = parent.Height;
            }

            if (!double.IsNaN(parent.Width))
            {
                ((FrameworkElement)sender).Width = parent.Width;
            }
        }
    }
}
