using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CommonPluginsControls.Controls
{
    /// <summary>
    /// Logique d'interaction pour OptionsDownloadData.xaml
    /// </summary>
    public partial class OptionsDownloadData : UserControl
    {
        private List<Game> FilteredGames { get; set; }
        private IPluginDatabase PluginDatabase { get; }


        public OptionsDownloadData(IPluginDatabase pluginDatabase, bool withoutMissing = false)
        {
            PluginDatabase = pluginDatabase;

            InitializeComponent();

            if (withoutMissing)
            {
                PART_OnlyMissing.Visibility = Visibility.Collapsed;
                PART_BtDownload.Content = ResourceProvider.GetString("LOCGameTagsTitle");
            }
            else
            {
                PART_TagMissing.Visibility = Visibility.Collapsed;
            }
        }


        private void PART_BtClose_Click(object sender, RoutedEventArgs e)
        {
            ((Window)Parent).Close();
        }

        private void PART_BtDownload_Click(object sender, RoutedEventArgs e)
        {
            FilteredGames = API.Instance.Database.Games.Where(x => x.Hidden == false).ToList();
            int months = (int)PART_Months.LongValue;

            if ((bool)PART_AllGames.IsChecked)
            {
            }

            if ((bool)PART_Filtred.IsChecked)
            {
                FilteredGames = API.Instance.MainView.FilteredGames;
            }

            if ((bool)PART_Selected.IsChecked)
            {
                FilteredGames = API.Instance.MainView.SelectedGames.ToList();
            }

            if ((bool)PART_GamesRecentlyPlayed.IsChecked)
            {
                FilteredGames = FilteredGames.Where(x => x.LastActivity != null && (DateTime)x.LastActivity >= DateTime.Now.AddMonths(-months)).ToList();
            }

            if ((bool)PART_GamesRecentlyAdded.IsChecked)
            {
                FilteredGames = FilteredGames.Where(x => x.Added != null && (DateTime)x.Added >= DateTime.Now.AddMonths(-months)).ToList();
            }

            if ((bool)PART_GamesInstalled.IsChecked)
            {
                FilteredGames = FilteredGames.Where(x => x.IsInstalled).ToList();
            }

            if ((bool)PART_GamesNotInstalled.IsChecked)
            {
                FilteredGames = FilteredGames.Where(x => !x.IsInstalled).ToList();
            }

            if ((bool)PART_GamesFavorite.IsChecked)
            {
                FilteredGames = FilteredGames.Where(x => x.Favorite).ToList();
            }

            if ((bool)PART_OldData.IsChecked)
            {
                FilteredGames = PluginDatabase.GetGamesOldData(months).ToList();
            }

            ((Window)Parent).Close();
        }


        public List<Game> GetFilteredGames()
        {
            return FilteredGames;
        }

        public bool GetTagMissing()
        {
            return (bool)PART_TagMissing.IsChecked;
        }

        public bool GetOnlyMissing()
        {
            return (bool)PART_OnlyMissing.IsChecked;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Part_MonthSelect.IsEnabled = (bool)PART_OldData.IsChecked || (bool)PART_GamesRecentlyAdded.IsChecked || (bool)PART_GamesRecentlyPlayed.IsChecked;
            PART_OnlyMissing.IsEnabled = !(bool)PART_OldData.IsChecked;
        }
    }
}
