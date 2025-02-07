using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
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
        private LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;
        internal override IPluginDatabase pluginDatabase => PluginDatabase;

        private PluginFlagsDataContext ControlDataContext = new PluginFlagsDataContext();
        internal override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginFlagsDataContext)controlDataContext;
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
                    API.Instance.Database.Games.ItemUpdated += Games_ItemUpdated;

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


        public override void SetData(Game newContext, PluginDataBaseGameBase pluginGameData)
        {
            GameLocalizations gameLocalization = (GameLocalizations)pluginGameData;

            List<GameLanguage> TaggedLanguage = PluginDatabase.PluginSettings.Settings.GameLanguages
                .FindAll(x => x.IsTag && gameLocalization.Items.Any(y => x.Name.IsEqual(y.Language)));

            ObservableCollection<ItemList> itemLists = new ObservableCollection<ItemList>();

            itemLists = gameLocalization.Items
                .Where(x => (!PluginDatabase.PluginSettings.Settings.OnlyDisplaySelectedFlags || TaggedLanguage.Any(y => x.Language.IsEqual(y.Name)))
                        && (!PluginDatabase.PluginSettings.Settings.OnlyDisplayExistingFlags || x.IsKnowFlag))
                .Select(x => new ItemList { Name = x.DisplayName, Icon = x.FlagIcon }).ToObservable();

            ControlDataContext.CountItems = itemLists.Count;
            ControlDataContext.ItemsSource = itemLists;
        }
    }


    public class PluginFlagsDataContext : ObservableObject, IDataContext
    {
        private bool _isActivated;
        public bool IsActivated { get => _isActivated; set => SetValue(ref _isActivated, value); }

        public int _countItems;
        public int CountItems { get => _countItems; set => SetValue(ref _countItems, value); }

        public ObservableCollection<ItemList> _itemsSource;
        public ObservableCollection<ItemList> ItemsSource { get => _itemsSource; set => SetValue(ref _itemsSource, value); }
    }

    public class ItemList
    {
        public string Name { get; set; }
        public BitmapImage Icon { get; set; }
    }
}
