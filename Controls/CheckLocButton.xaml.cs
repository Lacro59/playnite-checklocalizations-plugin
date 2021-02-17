using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CheckLocalizations.Views;
using CommonPluginsShared;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
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
    /// Logique d'interaction pour CheckLocButton.xaml
    /// </summary>
    public partial class CheckLocButton : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private CheckLocListLanguages PART_ListViewLanguages;

        private readonly string IconDefault = "";
        private readonly string IconOk = "";
        private readonly string IconKo = "";
        private readonly string IconNone = "";


        public CheckLocButton()
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
            // ContextMenu or Show Window
            PART_ContextMenu.Items.Clear();
            PART_ListViewLanguages = null;
            if (PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu)
            {
                PART_ListViewLanguages = new CheckLocListLanguages();
                PART_ListViewLanguages.WithColNotes = false;
                PART_ListViewLanguages.IgnoreSettings = true;
                PART_ContextMenu.Items.Add(PART_ListViewLanguages);
            }

            // Apply settings
            this.DataContext = new
            {
                PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonDetails,
                PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu
            };
            
            // Publish changes for the currently displayed game
            GameContextChanged(null, GameContext);
        }

        // When game is changed
        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            MustDisplay = PluginDatabase.PluginSettings.Settings.EnableIntegrationButton;

            // When control is not used
            if (!PluginDatabase.PluginSettings.Settings.EnableIntegrationButton)
            {
                return;
            }

            if (newContext != null && PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonDetails)
            {
                GameLocalizations gameLocalization = PluginDatabase.Get(newContext.Id, true);

                if (gameLocalization.Items.Count == 0)
                {
                    PART_ButtonIcon.Text = IconNone;
                }
                else
                {
                    if (gameLocalization.HasNativeSupport())
                    {
                        PART_ButtonIcon.Text = IconOk;
                    }
                    else
                    {
                        PART_ButtonIcon.Text = IconKo;
                    }
                }
            }
            else
            {
                PART_ButtonIcon.Text = IconDefault;
            }
        }
        #endregion


        private void PART_CheckLocButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu)
            {
                var ViewExtension = new CheckLocalizationsView();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginDatabase.PlayniteApi, "CheckLocalizations", ViewExtension);
                windowExtension.ShowDialog();
            }
        }

        private void PART_ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            if (!PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu)
            {
                PART_ContextMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                PART_ContextMenu.Visibility = Visibility.Visible;
                if (PART_ListViewLanguages != null)
                {
                    PART_ListViewLanguages.GameContext = GameContext;
                }
            }

            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)(sender)))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
    }
}
