using CommonPluginsShared;
using CommonPluginsShared.Interfaces;
using CommonPluginsShared.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonPluginsControls.Views
{
    /// <summary>
    /// Logique d'interaction pour ListDataWithoutGame.xaml
    /// </summary>
    public partial class ListDataWithoutGame : UserControl
    {
        private IPluginDatabase PluginDatabase { get; set; }
        private List<DataGame> DataPluginGames { get; set; }

        public ListDataWithoutGame(List<DataGame> DataPluginGames, IPluginDatabase PluginDatabase)
        {
            this.PluginDatabase = PluginDatabase;
            this.DataPluginGames = Serialization.GetClone(DataPluginGames.OrderBy(x => x.Name).ToList());
            
            InitializeComponent();

            PART_Lb.ItemsSource = this.DataPluginGames;
            PART_Count.Content = DataPluginGames.Count;
        }


        private void PART_BtRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirumationAskGeneric"), PluginDatabase.PluginName, MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    var data = (DataGame)PART_Lb.SelectedItem;
                    PART_Lb.ItemsSource = null;
                    PluginDatabase.Remove(data.Id);
                    DataPluginGames.Remove(data);
                    PART_Lb.ItemsSource = DataPluginGames;
                    PART_Count.Content = DataPluginGames.Count;

                    PART_BtRemove.IsEnabled = false;
                    PART_BtTransfer.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
                ((Window)this.Parent).Close();
            }
        }

        private void PART_BtTransfer_Click(object sender, RoutedEventArgs e)
        {
            WindowOptions windowOptions = new WindowOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = false,
                ShowCloseButton = true,
            };

            var data = (DataGame)PART_Lb.SelectedItem;

            TransfertData ViewExtension = new TransfertData(data, PluginDatabase);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCommonSelectTransferData"), ViewExtension, windowOptions);
            _ = windowExtension.ShowDialog();
        }

        private void PART_BtClose_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }
        

        private void PART_Lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PART_BtRemove.IsEnabled = true;
            PART_BtTransfer.IsEnabled = true;
        }
    }
}
