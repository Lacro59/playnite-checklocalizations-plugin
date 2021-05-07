using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logique d'interaction pour PluginFlags.xaml
    /// </summary>
    public partial class PluginFlags : PluginUserControlExtend
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

        private PluginFlagsDataContext ControlDataContext;
        internal override IDataContext _ControlDataContext
        {
            get
            {
                return ControlDataContext;
            }
            set
            {
                ControlDataContext = (PluginFlagsDataContext)_ControlDataContext;
            }
        }


        private readonly string IconOk = "\uea32";
        private readonly string IconKo = "\uea31";
        private readonly string IconNone = "\uea30";

        public PluginFlags()
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


        public override void SetDefaultDataContext()
        {
            ControlDataContext = new PluginFlagsDataContext
            {
                IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationFlags,

                CountItems = 0,
                ItemsSource = new ObservableCollection<ItemList>()
            };
        }


        public override Task<bool> SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            return Task.Run(() =>
            {
                GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

                ObservableCollection<ItemList> itemLists = new ObservableCollection<ItemList>();
                itemLists = gameLocalization.Items.Select(x => new ItemList { Name = x.DisplayName, Icon = x.FlagIcon }).ToObservable();

                ControlDataContext.CountItems = itemLists.Count;
                ControlDataContext.ItemsSource = itemLists;

                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                {
                    this.DataContext = ControlDataContext;
                }));

                return true;
            });
        }
    }


    public class PluginFlagsDataContext : IDataContext
    {
        public bool IsActivated { get; set; }

        public int CountItems { get; set; }
        public ObservableCollection<ItemList> ItemsSource { get; set; }
    }

    public class ItemList
    {
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
    }
}
