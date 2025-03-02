using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CheckLocalizations.Views;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
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
        private LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;
        internal override IPluginDatabase pluginDatabase => PluginDatabase;

        private PluginButtonDataContext ControlDataContext = new PluginButtonDataContext();
        internal override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginButtonDataContext)controlDataContext;
        }


        private PluginListLanguages PART_ListViewLanguages;

        private readonly string IconDefault = "\uea2c";
        private readonly string IconOk = "\uea32";
        private readonly string IconKo = "\uea31";
        private readonly string IconNone = "\uea30";


        public PluginButton()
        {
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
                    API.Instance.Database.Games.ItemUpdated += Games_ItemUpdated;

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


        public override void SetData(Game newContext, PluginDataBaseGameBase pluginGameData)
        {
            GameLocalizations gameLocalization = (GameLocalizations)pluginGameData;

            ControlDataContext.Text = ControlDataContext.DisplayDetails
                ? gameLocalization.Items.Count == 0
                    ? IconNone
                    : gameLocalization.HasNativeSupport() ? IconOk : IconKo
                : IconDefault;
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
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginDatabase.PluginName, ViewExtension);
                _ = windowExtension.ShowDialog();
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
                        Width = 600,
                        Height = 150
                    };
                    PART_ListViewLanguages.Margin = new Thickness(0, 5, 0, 5);
                    _ = PART_ContextMenu.Items.Add(PART_ListViewLanguages);
                }

                PART_ContextMenu.Visibility = Visibility.Visible;
                if (PART_ListViewLanguages != null)
                {
                    PART_ListViewLanguages.GameContext = GameContext;
                }
            }

            foreach (Border ui in UI.FindVisualChildren<Border>((ContextMenu)sender))
            {
                if (ui.Name == "HoverBorder")
                {
                    ui.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
        #endregion
    }


    public class PluginButtonDataContext : ObservableObject, IDataContext
    {
        private bool _isActivated;
        public bool IsActivated { get => _isActivated; set => SetValue(ref _isActivated, value); }

        public bool _displayDetails;
        public bool DisplayDetails { get => _displayDetails; set => SetValue(ref _displayDetails, value); }

        public bool _buttonContextMenu;
        public bool ButtonContextMenu { get => _buttonContextMenu; set => SetValue(ref _buttonContextMenu, value); }

        public string _text = "\uea2c";
        public string Text { get => _text; set => SetValue(ref _text, value); }
    }
}
