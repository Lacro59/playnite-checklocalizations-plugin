using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CheckLocalizations.Views;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CheckLocalizations.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginButton.xaml
    /// </summary>
    public partial class PluginButton : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;
        internal override IPluginDatabase _PluginDatabase
        {
            get => PluginDatabase;
            set => PluginDatabase = (LocalizationsDatabase)_PluginDatabase;
        }

        private PluginButtonDataContext ControlDataContext = new PluginButtonDataContext();
        internal override IDataContext _ControlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginButtonDataContext)_ControlDataContext;
        }


        private PluginListLanguages PART_ListViewLanguages;

        private readonly string IconDefault = "\uea2c";
        private readonly string IconOk = "\uea32";
        private readonly string IconKo = "\uea31";
        private readonly string IconNone = "\uea30";


        public PluginButton()
        {
            AlwaysShow = true;

            InitializeComponent();
            this.DataContext = ControlDataContext;

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


        public override void SetDefaultDataContext()
        {
            ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationButton;
            ControlDataContext.DisplayDetails = PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonDetails;
            ControlDataContext.ButtonContextMenu = PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu;

            ControlDataContext.Text = string.Empty;
        }


        public override void SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

            if (ControlDataContext.DisplayDetails)
            {
                ControlDataContext.Text = gameLocalization.Items.Count == 0 
                    ? IconNone 
                    : gameLocalization.HasNativeSupport() ? IconOk : IconKo;
            }
            else
            {
                ControlDataContext.Text = IconDefault;
            }
        }


        #region OnPropertyChange
        // When settings is updated
        internal override void PluginSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ControlDataContext == null)
            {
                return;
            }

            // ContextMenu or show Window
            PART_ContextMenu.Items.Clear();
            PART_ListViewLanguages = null;
            
            // Publish changes for the currently displayed game
            GameContextChanged(null, GameContext);
        }
        #endregion


        #region Events
        private void PART_PluginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu)
            {
                CheckLocalizationsView ViewExtension = new CheckLocalizationsView();
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginDatabase.PlayniteApi, PluginDatabase.PluginName, ViewExtension);
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
                if (PART_ContextMenu.Items.Count == 0)
                {
                    PART_ListViewLanguages = new PluginListLanguages
                    {
                        WithColNotes = false,
                        IgnoreSettings = true,
                        Width = 450,
                        Height = 150
                    };
                    PART_ListViewLanguages.Margin = new Thickness(0, 5, 0, 5);
                    PART_ContextMenu.Items.Add(PART_ListViewLanguages);
                }

                PART_ContextMenu.Visibility = Visibility.Visible;
                if (PART_ListViewLanguages != null)
                {
                    PART_ListViewLanguages.GameContext = GameContext;
                }
            }

            foreach (var ui in UI.FindVisualChildren<Border>((ContextMenu)(sender)))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        #endregion
    }


    public class PluginButtonDataContext : ObservableObject, IDataContext
    {
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

        public bool _DisplayDetails;
        public bool DisplayDetails { get => _DisplayDetails; set => SetValue(ref _DisplayDetails, value); }

        public bool _ButtonContextMenu;
        public bool ButtonContextMenu { get => _ButtonContextMenu; set => SetValue(ref _ButtonContextMenu, value); }

        public string _Text;
        public string Text { get => _Text; set => SetValue(ref _Text, value); }
    }
}
