using Playnite.SDK;
using CommonPluginsShared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonPluginsShared.Interfaces;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using Playnite.SDK.Data;

namespace CommonPluginsControls.Views
{
    /// <summary>
    /// Logique d'interaction pour TransfertData.xaml
    /// </summary>
    public partial class TransfertData : UserControl
    {
        internal static readonly ILogger logger = LogManager.GetLogger();
        private IPluginDatabase PluginDatabase { get; set; }


        public TransfertData(List<DataGame> DataPluginGames, IPluginDatabase PluginDatabase)
        {
            this.PluginDatabase = PluginDatabase;
            Init(DataPluginGames);
        }

        public TransfertData(DataGame DataPluginGame, IPluginDatabase PluginDatabase)
        {
            this.PluginDatabase = PluginDatabase;
            Init(new List<DataGame> { DataPluginGame });
            PART_CbPluginGame.SelectedIndex = 0;
            PART_CbPluginGame.IsEnabled = false;
        }


        private void Init(List<DataGame> DataPluginGames)
        {
            InitializeComponent();

            List<DataGame> DataGames = API.Instance.Database.Games.Where(x => !x.Hidden).Select(x => new DataGame
            {
                Id = x.Id,
                Icon = x.Icon.IsNullOrEmpty() ? x.Icon : API.Instance.Database.GetFullFilePath(x.Icon),
                Name = x.Name,
                CountData = PluginDatabase.Get(x.Id, true)?.Count ?? 0
            }).Distinct().ToList();

            PART_CbPluginGame.ItemsSource = DataPluginGames.OrderBy(x => x.Name).ToList();
            PART_CbGame.ItemsSource = DataGames.OrderBy(x => x.Name).ToList();
        }


        private void PART_BtClose_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }

        private void PART_BtTransfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PluginDataBaseGameBase PluginData;

                if ((bool)Part_Merged.IsChecked)
                {
                    PluginData = PluginDatabase.MergeData(((DataGame)PART_CbPluginGame.SelectedItem).Id, ((DataGame)PART_CbGame.SelectedItem).Id);
                }
                else
                {
                    PluginData = PluginDatabase.GetClone(((DataGame)PART_CbPluginGame.SelectedItem).Id);
                    PluginData.Id = ((DataGame)PART_CbGame.SelectedItem).Id;
                    PluginData.Name = ((DataGame)PART_CbGame.SelectedItem).Name;
                    PluginData.Game = API.Instance.Database.Games.Get(((DataGame)PART_CbGame.SelectedItem).Id);
                }

                if (PluginData != null)
                {
                    PluginDatabase.AddOrUpdate(PluginData);
                }
                else
                {
                    logger.Warn($"{PluginDatabase.PluginName} - No data saved from {((DataGame)PART_CbPluginGame.SelectedItem).Name} to {((DataGame)PART_CbGame.SelectedItem).Name}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }

            ((Window)this.Parent).Close();
        }


        private void PART_Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PART_BtTransfer.IsEnabled = PART_CbPluginGame.SelectedIndex == -1 || PART_CbGame.SelectedIndex == -1
                ? false
                : ((DataGame)PART_CbPluginGame.SelectedItem).Id != ((DataGame)PART_CbGame.SelectedItem).Id;
        }
    }
}
