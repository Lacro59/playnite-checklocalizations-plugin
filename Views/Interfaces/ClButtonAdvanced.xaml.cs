using CheckLocalizations.Models;
using Playnite.SDK;
using PluginCommon;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using CheckLocalizations.Services;
using Newtonsoft.Json;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClButtonAdvanced.xaml
    /// </summary>
    public partial class ClButtonAdvanced : Button
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private static readonly string IsTextDefault = " ";
        private static readonly string IsTextOk = "";
        private static readonly string IsTextKo = "";
        private static readonly string IsTextNone = "";

        private static readonly string OnlyIconIsDefault = "";
        private static readonly string OnlyIconIsOk = "";
        private static readonly string OnlyIconIsKo = "";
        private static readonly string OnlyIconIsNone = "";

        private ClListViewLanguages PART_ListViewLanguages;

        bool? _JustIcon = null;


        public ClButtonAdvanced(bool? JustIcon = null)
        {
            _JustIcon = JustIcon;

            InitializeComponent();

            PART_ListViewLanguages = new ClListViewLanguages(false);
            PART_ContextMenu.Items.Add(PART_ListViewLanguages);

            bool EnableIntegrationButtonJustIcon;
            if (_JustIcon == null)
            {
                EnableIntegrationButtonJustIcon = PluginDatabase.PluginSettings.EnableIntegrationButtonJustIcon;
            }
            else
            {
                EnableIntegrationButtonJustIcon = (bool)_JustIcon;
            }

            this.DataContext = new
            {
                EnableIntegrationButtonJustIcon = EnableIntegrationButtonJustIcon
            };
#if DEBUG
            logger.Debug($"CheckLocalizations - DataContext: {JsonConvert.SerializeObject(DataContext)}");
#endif

            PluginDatabase.PropertyChanged += OnPropertyChanged;
        }


        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "GameSelectedData" || e.PropertyName == "PluginSettings")
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        if (PluginDatabase.GameSelectedData.HasNativeSupport())
                        {
                            PART_ButtonText.Text = IsTextOk;
                            PART_ButtonIcon.Text = OnlyIconIsOk;
                        }
                        else
                        {
                            PART_ButtonText.Text = IsTextKo;
                            PART_ButtonIcon.Text = OnlyIconIsKo;
                        }

                        if (CheckLocalizations.PluginDatabase.GameSelectedData.Items.Count == 0)
                        {
                            PART_ButtonText.Text = IsTextNone;
                            PART_ButtonIcon.Text = OnlyIconIsNone;
                        }


                        bool EnableIntegrationButtonJustIcon;
                        if (_JustIcon == null)
                        {
                            EnableIntegrationButtonJustIcon = PluginDatabase.PluginSettings.EnableIntegrationButtonJustIcon;
                        }
                        else
                        {
                            EnableIntegrationButtonJustIcon = (bool)_JustIcon;
                        }

                        this.DataContext = new
                        {
                            EnableIntegrationButtonJustIcon = EnableIntegrationButtonJustIcon
                        };
                    }));
                }
                else
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        PART_ButtonIcon.Text = IsTextDefault;
                        PART_ButtonIcon.Text = OnlyIconIsDefault;
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }


        // Design popup
        private void PART_ContextMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            /*
            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)(sender)))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
            */
        }
    }
}
