using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CommonPluginsControls.Views
{
    /// <summary>
    /// Logique d'interaction pour ListDataUpdated.xaml
    /// </summary>
    public partial class ListDataUpdated : UserControl
    {
        private List<GameUpdatedData> gameUpdatedDatas = new List<GameUpdatedData>();
        RelayCommand<Guid> GoToGame { get; set; }


        public ListDataUpdated(List<Game> games)
        {
            InitializeComponent();


            GoToGame = new RelayCommand<Guid>((Id) =>
            {
                API.Instance.MainView.SelectGame(Id);
                API.Instance.MainView.SwitchToLibraryView();
            });


            gameUpdatedDatas = games.Select(x => new GameUpdatedData { Id = x.Id, Name = x.Name, GoToGame = GoToGame }).ToList();
            ListViewGames.ItemsSource = gameUpdatedDatas;

            PART_Count.Content = gameUpdatedDatas.Count;

            ListViewGames.Sorting();
        }
    }


    public class GameUpdatedData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RelayCommand<Guid> GoToGame { get; set; }
    }
}
