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
    /// Logique d'interaction pour CheckLocViewItem.xaml
    /// </summary>
    public partial class CheckLocViewItem : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private readonly string IconDefault = "";
        private readonly string IconOk = "";
        private readonly string IconKo = "";
        private readonly string IconNone = "";


        public CheckLocViewItem()
        {
            InitializeComponent();

            PluginDatabase.PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
            PluginDatabase.Database.ItemUpdated += Database_ItemUpdated;
            PluginDatabase.PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;

            // Apply settings
            PluginSettings_PropertyChanged(null, null);
        }

        #region OnPropertyChange
        // When settings is updated
        public override void PluginSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Publish changes for the currently displayed game
            GameContextChanged(null, GameContext);
        }

        // When game is changed
        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            MustDisplay = PluginDatabase.PluginSettings.Settings.EnableIntegrationViewItem;
            
            // When control is not used
            if (!PluginDatabase.PluginSettings.Settings.EnableIntegrationViewItem)
            {
                return;
            }

            if (newContext != null)
            {
                GameLocalizations gameLocalization = PluginDatabase.Get(newContext.Id, true);

                if (gameLocalization.Items.Count == 0)
                {
                    PART_Icon.Text = IconNone;
                }
                else
                {
                    if (gameLocalization.HasNativeSupport())
                    {
                        PART_Icon.Text = IconOk;
                    }
                    else
                    {
                        PART_Icon.Text = IconKo;
                    }
                }
            }
            else
            {
                this.MustDisplay = false;
            }
        }
        #endregion
    }
}
