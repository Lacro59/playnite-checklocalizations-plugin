using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CommonPluginsControls.Views
{
    /// <summary>
    /// Logique d'interaction pour ListWithNoData.xaml
    /// </summary>
    public partial class ListWithNoData : UserControl
    {
        private IPluginDatabase PluginDatabase { get; set; }
        private List<GameData> gameData = new List<GameData>();

        private RelayCommand<Guid> GoToGame { get; set; }

        public ListWithNoData(IPluginDatabase PluginDatabase)
        {
            this.PluginDatabase = PluginDatabase;

            InitializeComponent();

            GoToGame = new RelayCommand<Guid>((Id) =>
            {
                API.Instance.MainView.SelectGame(Id);
                API.Instance.MainView.SwitchToLibraryView();
            });

            RefreshData();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.Refresh(gameData.Select(x => x.Id));
            RefreshData();
        }


        private void RefreshData()
        {
            ListViewGames.ItemsSource = null;
            IEnumerable<Game> games = PluginDatabase.GetGamesWithNoData();
            gameData = games.Select(x => new GameData { Id = x.Id, Name = x.Name, GoToGame = GoToGame }).ToList();
            ListViewGames.ItemsSource = gameData;

            PART_Count.Content = gameData.Count;

            ListViewGames.Sorting();
        }
    }


    public class GameData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RelayCommand<Guid> GoToGame { get; set; }
    }
}
