using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logique d'interaction pour PluginListLanguages.xaml
    /// </summary>
    public partial class PluginListLanguages : PluginUserControlExtend
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

        private PluginListLanguagesDataContext ControlDataContext;
        internal override IDataContext _ControlDataContext
        {
            get
            {
                return ControlDataContext;
            }
            set
            {
                ControlDataContext = (PluginListLanguagesDataContext)_ControlDataContext;
            }
        }


        #region Properties
        public static readonly DependencyProperty WithColNotesProperty;
        public bool? WithColNotes { get; set; }
        #endregion


        public PluginListLanguages()
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
            double ListLanguagesHeight = PluginDatabase.PluginSettings.Settings.ListLanguagesHeight;
            if (IgnoreSettings)
            {
                ListLanguagesHeight = double.NaN;
            }


            ControlDataContext = new PluginListLanguagesDataContext
            {
                IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguages,
                ListLanguagesHeight = ListLanguagesHeight,
                ListLanguagesVisibleEmpty = PluginDatabase.PluginSettings.Settings.ListLanguagesVisibleEmpty,

                ItemsSource = new ObservableCollection<Models.Localization>()
            };


            PART_GridContener_SizeChanged(null, null);
        }


        public override Task<bool> SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            bool IgnoreSettings = this.IgnoreSettings;
            bool MustDisplay = this.MustDisplay;

            return Task.Run(() =>
            {
                GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

                if (!IgnoreSettings && !ControlDataContext.ListLanguagesVisibleEmpty)
                {
                    MustDisplay = gameLocalization.HasData;
                }
                else
                {
                    MustDisplay = true;
                }

                if (MustDisplay)
                {
                    ControlDataContext.ItemsSource = gameLocalization.Items.ToObservable();
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                {
                    this.MustDisplay = MustDisplay;
                    this.DataContext = ControlDataContext;
                }));

                return true;
            });
        }


        #region Events
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
                WithColNotes = PluginDatabase.PluginSettings.Settings.ListLanguagesWithColNote;
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
        #endregion
    }


    public class PluginListLanguagesDataContext : IDataContext
    {
        public bool IsActivated { get; set; }
        public double ListLanguagesHeight { get; set; }
        public bool ListLanguagesVisibleEmpty { get; set; }

        public ObservableCollection<Models.Localization> ItemsSource { get; set; }
    }
}
