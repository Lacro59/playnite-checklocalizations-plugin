﻿using CheckLocalizations.Models;
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
using System.Windows;

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
            get => PluginDatabase;
            set => PluginDatabase = (LocalizationsDatabase)_PluginDatabase;
        }

        private PluginListLanguagesDataContext ControlDataContext = new PluginListLanguagesDataContext();
        internal override IDataContext _ControlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginListLanguagesDataContext)_ControlDataContext;
        }


        #region Properties
        public static readonly DependencyProperty WithColNotesProperty;
        public bool? WithColNotes { get; set; }
        #endregion


        public PluginListLanguages()
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
            bool IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationListLanguages;
            double ListLanguagesHeight = PluginDatabase.PluginSettings.Settings.ListLanguagesHeight;
            if (IgnoreSettings)
            {
                IsActivated = true;
                ListLanguagesHeight = double.NaN;
            }


            ControlDataContext.IsActivated = IsActivated;
            ControlDataContext.ListLanguagesHeight = ListLanguagesHeight;
            ControlDataContext.ListLanguagesVisibleEmpty = PluginDatabase.PluginSettings.Settings.ListLanguagesVisibleEmpty;

            ControlDataContext.ItemsSource = new ObservableCollection<Models.Localization>();


            PART_GridContener_SizeChanged(null, null);
        }


        public override void SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            GameLocalizations gameLocalization = (GameLocalizations)PluginGameData;

            MustDisplay = IgnoreSettings || ControlDataContext.ListLanguagesVisibleEmpty || gameLocalization.HasData;

            if (MustDisplay)
            {
                ControlDataContext.ItemsSource = gameLocalization.Items.ToObservable();
            }
        }


        #region Events
        private void PART_GridContener_Loaded(object sender, RoutedEventArgs e)
        {
            PART_GridContener_SizeChanged(null, null);
        }

        private void PART_GridContener_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bool WithColNotes = this.WithColNotes != null ? (bool)this.WithColNotes : PluginDatabase.PluginSettings.Settings.ListLanguagesWithColNote;
            if (WithColNotes)
            {
                PART_ColNotes.Width = 100;
                if (!double.IsNaN(PART_ListViewLanguages.ActualWidth))
                {
                    double Width = PART_ListViewLanguages.ActualWidth - PART_Name.ActualWidth
                        - PART_Ui.ActualWidth - PART_Audio.ActualWidth - PART_Sub.ActualWidth
                        - 30 - 25;

                    PART_ColNotes.Width = Width > 50 ? Width : 50;
                }
            }
            else
            {
                PART_ColNotes.Width = 0;
            }
        }
        #endregion
    }


    public class PluginListLanguagesDataContext : ObservableObject, IDataContext
    {
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

        private double _ListLanguagesHeight = 120;
        public double ListLanguagesHeight { get => _ListLanguagesHeight; set => SetValue(ref _ListLanguagesHeight, value); }

        private bool _ListLanguagesVisibleEmpty;
        public bool ListLanguagesVisibleEmpty { get => _ListLanguagesVisibleEmpty; set => SetValue(ref _ListLanguagesVisibleEmpty, value); }

        private ObservableCollection<Models.Localization> _ItemsSource;
        public ObservableCollection<Models.Localization> ItemsSource { get => _ItemsSource; set => SetValue(ref _ItemsSource, value); }
    }
}
