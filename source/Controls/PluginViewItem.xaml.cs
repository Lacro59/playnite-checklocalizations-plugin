using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginViewItem.xaml
    /// </summary>
    public partial class PluginViewItem : PluginUserControlExtend
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;
        internal override IPluginDatabase _PluginDatabase
        {
            get => PluginDatabase;
            set => PluginDatabase = (LocalizationsDatabase)_PluginDatabase;
        }

        private PluginViewItemDataContext ControlDataContext =new PluginViewItemDataContext();
        internal override IDataContext _ControlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginViewItemDataContext)_ControlDataContext;
        }


        private readonly string IconOk = "\uea32";
        private readonly string IconKo = "\uea31";
        private readonly string IconNone = "\uea30";


        public PluginViewItem()
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
            ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationViewItem;
            ControlDataContext.Text = string.Empty;
        }


        public override void SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

            ControlDataContext.Text = gameLocalization.Items.Count == 0 
                ? IconNone 
                : gameLocalization.HasNativeSupport() ? IconOk : IconKo;
        }
    }


    public class PluginViewItemDataContext : ObservableObject, IDataContext
    {
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

        public string _Text;
        public string Text { get => _Text; set => SetValue(ref _Text, value); }
    }
}
