using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CheckLocalizations.Views;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
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
    /// Logique d'interaction pour PluginButton.xaml
    /// </summary>
    public partial class PluginButton : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;
        internal override IPluginDatabase _PluginDatabase
        {
            get
            {
                return PluginDatabase;
            }
            set
            {
                PluginDatabase = (LocalizationsDatabase)_PluginDatabase;
            }
        }

        private PluginButtonDataContext ControlDataContext;
        internal override IDataContext _ControlDataContext
        {
            get
            {
                return ControlDataContext;
            }
            set
            {
                ControlDataContext = (PluginButtonDataContext)_ControlDataContext;
            }
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
            ControlDataContext = new PluginButtonDataContext
            {
                IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationButton,
                DisplayDetails = PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonDetails,
                ButtonContextMenu = PluginDatabase.PluginSettings.Settings.EnableIntegrationButtonContextMenu,

                Text = string.Empty
            };
        }


        public override Task<bool> SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            return Task.Run(() =>
            {
                GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

                if (ControlDataContext.DisplayDetails)
                {
                    if (gameLocalization.Items.Count == 0)
                    {
                        ControlDataContext.Text = IconNone;
                    }
                    else
                    {
                        if (gameLocalization.HasNativeSupport())
                        {
                            ControlDataContext.Text = IconOk;
                        }
                        else
                        {
                            ControlDataContext.Text = IconKo;
                        }
                    }
                }
                else
                {
                    ControlDataContext.Text = IconDefault;
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                {
                    this.DataContext = ControlDataContext;
                }));

                return true;
            });
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

            if (ControlDataContext.ButtonContextMenu)
            {
                PART_ListViewLanguages = new PluginListLanguages
                {
                    WithColNotes = false,
                    IgnoreSettings = true,
                    Width = 450,
                    Height = 150
                };
                PART_ContextMenu.Items.Add(PART_ListViewLanguages);
            }
            
            // Publish changes for the currently displayed game
            GameContextChanged(null, GameContext);
        }
        #endregion


        #region Events
        private void PART_PluginButton_Click(object sender, RoutedEventArgs e)
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
        #endregion
    }


    public class PluginButtonDataContext : IDataContext
    {
        public bool IsActivated { get; set; }
        public bool DisplayDetails { get; set; }
        public bool ButtonContextMenu { get; set; }

        public string Text { get; set; }
    }
}
