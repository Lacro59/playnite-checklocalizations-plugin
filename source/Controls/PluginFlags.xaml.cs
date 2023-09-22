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
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
            get => PluginDatabase;
            set => PluginDatabase = (LocalizationsDatabase)_PluginDatabase;
        }

        private PluginFlagsDataContext ControlDataContext = new PluginFlagsDataContext();
        internal override IDataContext _ControlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginFlagsDataContext)_ControlDataContext;
        }

        public PluginFlags()
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
                    PluginDatabase.PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;

                    // Apply settings
                    PluginSettings_PropertyChanged(null, null);
                });
            });
        }


        public override void SetDefaultDataContext()
        {
            ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationFlags;

            ControlDataContext.CountItems = 0;
            ControlDataContext.ItemsSource = new ObservableCollection<ItemList>();
        }


        public override void SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

            ObservableCollection<ItemList> itemLists = new ObservableCollection<ItemList>();
            if (PluginDatabase.PluginSettings.Settings.OnlyDisplaySelectedFlags)
            {
                List<GameLanguage> TaggedLanguage = PluginDatabase.PluginSettings.Settings.GameLanguages
                    .FindAll(x => x.IsTag && gameLocalization.Items.Any(y => x.Name.ToLower() == y.Language.ToLower()));

                itemLists = gameLocalization.Items
                    .Where(x => TaggedLanguage.Any(y => x.Language.ToLower() == y.Name.ToLower()))
                    .Select(x => new ItemList { Name = x.DisplayName, Icon = x.FlagIcon }).ToObservable();
            }
            else
            {
                itemLists = gameLocalization.Items.Select(x => new ItemList { Name = x.DisplayName, Icon = x.FlagIcon }).ToObservable();
            }

            ControlDataContext.CountItems = itemLists.Count;
            ControlDataContext.ItemsSource = itemLists;
        }
    }


    public class PluginFlagsDataContext : ObservableObject, IDataContext
    {
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

        public int _CountItems;
        public int CountItems { get => _CountItems; set => SetValue(ref _CountItems, value); }

        public ObservableCollection<ItemList> _ItemsSource;
        public ObservableCollection<ItemList> ItemsSource { get => _ItemsSource; set => SetValue(ref _ItemsSource, value); }
    }

    public class ItemList
    {
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
    }
}
