using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared.Controls;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace CheckLocalizations.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginViewItem.xaml
    /// </summary>
    public partial class PluginViewItem : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private readonly string IconDefault = "";
        private readonly string IconOk = "";
        private readonly string IconKo = "";
        private readonly string IconNone = "";


        public PluginViewItem()
        {
            InitializeComponent();

            Task.Run(() =>
            {
                // Wait extension database are loaded
                System.Threading.SpinWait.SpinUntil(() => PluginDatabase.IsLoaded, -1);

                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    PluginDatabase.PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
                    PluginDatabase.Database.ItemUpdated += Database_ItemUpdated;
                    PluginDatabase.Database.ItemCollectionChanged += Database_ItemCollectionChanged;
                    PluginDatabase.PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;

                    // Apply settings
                    PluginSettings_PropertyChanged(null, null);
                });
            });
        }

        #region OnPropertyChange
        // When settings is updated
        public override void PluginSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
            // Apply settings
            this.DataContext = new
            {

            };

            // Publish changes for the currently displayed game
            GameContextChanged(null, GameContext);
        }

        // When game is changed
        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (!PluginDatabase.IsLoaded)
            {
                return;
            }

            MustDisplay = PluginDatabase.PluginSettings.Settings.EnableIntegrationViewItem;
            
            // When control is not used
            if (!PluginDatabase.PluginSettings.Settings.EnableIntegrationViewItem)
            {
                return;
            }

            // Default value
            string Text = string.Empty;

            if (newContext != null)
            {
                GameLocalizations gameLocalization = PluginDatabase.Get(newContext.Id, true);

                if (gameLocalization.Items.Count == 0)
                {
                    Text = IconNone;
                }
                else
                {
                    if (gameLocalization.HasNativeSupport())
                    {
                        Text = IconOk;
                    }
                    else
                    {
                        Text = IconKo;
                    }
                }
            }
            else
            {
                MustDisplay = false;
            }

            this.DataContext = new
            {
                Text
            };
        }
        #endregion
    }
}
