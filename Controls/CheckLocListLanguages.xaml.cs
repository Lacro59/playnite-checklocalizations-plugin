using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared;
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
    /// Logique d'interaction pour CheckLocListLanguages.xaml
    /// </summary>
    public partial class CheckLocListLanguages : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;


        public static readonly DependencyProperty WithColNotesProperty;
        public bool? WithColNotes { get; set; }

        public static readonly DependencyProperty IgnoreSettingsProperty;
        public bool IgnoreSettings { get; set; }


        public CheckLocListLanguages()
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
            // Apply settings
            this.DataContext = new
            {
                
            };

            PART_GridContener_SizeChanged(null, null);

            // Publish changes for the currently displayed game
            GameContextChanged(null, PluginDatabase.GameContext);
        }

        // When game is changed
        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (IgnoreSettings)
            {
                MustDisplay = true;
            }
            else
            {
                MustDisplay = PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguages;
            }

            // When control is not used
            if (!IgnoreSettings && !PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguages)
            {
                return;
            }

            if (newContext != null)
            {
                GameLocalizations gameLocalization = PluginDatabase.Get(newContext.Id, true);

                PART_ListViewLanguages.ItemsSource = null;
                PART_ListViewLanguages.ItemsSource = gameLocalization.Items;


                if (!IgnoreSettings && !PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguagesVisibleEmpty)
                {
                    MustDisplay = gameLocalization.HasData;
                }
            }
            else
            {
                if (!IgnoreSettings)
                {
                    MustDisplay = PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguagesVisibleEmpty;
                }
            }
        }
        #endregion


        private void PART_GridContener_Loaded(object sender, RoutedEventArgs e)
        {
            PART_GridContener_SizeChanged(null, null);
        }

        private void PART_GridContener_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bool WithColNotes = true; ;
            if (this.WithColNotes != null)
            {
                WithColNotes = (bool)this.WithColNotes;
            }
            else
            {
                WithColNotes = PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguagesWithColNote;
            }

            if (WithColNotes)
            {
                PART_ColNotes.Width = 100;
                if (!double.IsNaN(PART_ListViewLanguages.ActualWidth))
                {
                    double Width = PART_ListViewLanguages.ActualWidth - PART_Name.ActualWidth
                        - PART_Ui.ActualWidth - PART_Audio.ActualWidth - PART_Sub.ActualWidth
                        - 30 - 25;

                    if (Width > 50)
                    {
                        PART_ColNotes.Width = Width;
                    }
                    else
                    {
                        PART_ColNotes.Width = 50;
                    }
                }
            }
            else
            {
                PART_ColNotes.Width = 0;
            }
        }
    }
}
