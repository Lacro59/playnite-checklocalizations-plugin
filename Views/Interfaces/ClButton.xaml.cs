using CheckLocalizations.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using PluginCommon;
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

            PluginDatabase.PropertyChanged += OnPropertyChanged;
        }


        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
#if DEBUG
                logger.Debug($"ClButton.OnPropertyChanged({e.PropertyName}): {JsonConvert.SerializeObject(PluginDatabase.GameSelectedData)}");
#endif
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

                        if (EnableIntegrationButtonJustIcon)
                        {
                            OnlyIcon.Visibility = Visibility.Visible;
                            IndicatorSupportText.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            OnlyIcon.Visibility = Visibility.Collapsed;
                            IndicatorSupportText.Visibility = Visibility.Visible;
                        }
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
