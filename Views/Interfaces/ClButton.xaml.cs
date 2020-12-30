using CheckLocalizations.Services;
using CommonPluginsShared;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClButton.xaml
    /// </summary>
    public partial class ClButton : Button
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        bool? _JustIcon = null;


        public ClButton(bool? JustIcon = null)
        {
            _JustIcon = JustIcon;

            InitializeComponent();


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


            PluginDatabase.PropertyChanged += OnPropertyChanged;
        }


        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "PluginSettings")
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
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
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }
    }
}
