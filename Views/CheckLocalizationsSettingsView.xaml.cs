using CheckLocalizations.Models;
using CheckLocalizations.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.API;
using PluginCommon.PlayniteResources.Common;
using PluginCommon.PlayniteResources.Converters;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace CheckLocalizations.Views
{
    public partial class CheckLocalizationsSettingsView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private IPlayniteAPI PlayniteApi { get; set; }
        private CheckLocalizationsSettings settings { get; set; }
        private string PluginUserDataPath { get; set; }

        private LocalizationsApi localizationsApi { get; set; }

        public static bool WithoutMessage = false;
        public static CancellationTokenSource tokenSource;
        private CancellationToken ct;


        public CheckLocalizationsSettingsView(string PluginUserDataPath, IPlayniteAPI PlayniteApi, CheckLocalizationsSettings settings)
        {
            this.PlayniteApi = PlayniteApi;
            this.settings = settings;
            this.PluginUserDataPath = PluginUserDataPath;

            localizationsApi = new LocalizationsApi(PluginUserDataPath, PlayniteApi, settings);

            InitializeComponent();

            DataLoad.Visibility = Visibility.Collapsed;

            lbGameLanguages.ItemsSource = settings.GameLanguages.OrderBy(x => x.DisplayName).ToList();

            DataContext = this;
        }

        private void ButtonCancelTask_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
        }

        /// <summary>
        /// Remove tag localizations in all games
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            pbDataLoad.IsIndeterminate = true;

            tbDataLoad.Text = resources.GetString("LOCCheckLocalizationsProgressBarTag");

            DataLoad.Visibility = Visibility.Visible;
            spSettings.Visibility = Visibility.Hidden;

            tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;

            var taskSystem = Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                foreach (Game game in PlayniteApi.Database.Games)
                {
                    try
                    {
                        localizationsApi.GetLocalizations(game, true);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, "CheckLocalizations", $"Error on ButtonAdd_Click() with {game.Name}");
                    }

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, tokenSource.Token)
            .ContinueWith(antecedent =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    DataLoad.Visibility = Visibility.Collapsed;
                    spSettings.Visibility = Visibility.Visible;
                }));
            });
        }

        /// <summary>
        /// Add tag localizations in all games
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            pbDataLoad.IsIndeterminate = true;

            tbDataLoad.Text = resources.GetString("LOCCheckLocalizationsProgressBarTag");

            DataLoad.Visibility = Visibility.Visible;
            spSettings.Visibility = Visibility.Hidden;

            tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;

            var taskSystem = Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                foreach (Game game in PlayniteApi.Database.Games)
                {
                    try
                    {
                        localizationsApi.RemoveTag(game);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, "CheckLocalizations", $"Error on ButtonRemove_Click() with {game.Name}");
                    }

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, tokenSource.Token)
            .ContinueWith(antecedent =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    DataLoad.Visibility = Visibility.Collapsed;
                    spSettings.Visibility = Visibility.Visible;
                }));
            });
        }

        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;

            if ((cb.Name == "CheckL_IntegrationInButton") && (bool)cb.IsChecked)
            {
                CheckL_IntegrationInCustomTheme.IsChecked = false;
            }

            if ((cb.Name == "CheckL_IntegrationInCustomTheme") && (bool)cb.IsChecked)
            {
                CheckL_IntegrationInButton.IsChecked = false;
            }
        }

        /// <summary>
        /// Get all games localizations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            // Clear before
            try
            {
                string PluginDirectory = PluginUserDataPath + "\\CheckLocalizations\\";
                if (Directory.Exists(PluginDirectory))
                {
                    Directory.Delete(PluginDirectory, true);
                    Directory.CreateDirectory(PluginDirectory);
                }
            }
            catch(Exception ex)
            {
                Common.LogError(ex, "CheckLocalization", $"Error on clear directory");
            }

            pbDataLoad.IsIndeterminate = false;
            pbDataLoad.Minimum = 0;
            pbDataLoad.Value = 0;
            pbDataLoad.Maximum = PlayniteApi.Database.Games.Count;

            int CountFind = 0;
            int CountNotFind = 0;
            tbDataLoad.Text = string.Format(resources.GetString("LOCCheckLocalizationsProgressBar"), CountFind, CountNotFind);

            DataLoad.Visibility = Visibility.Visible;
            spSettings.Visibility = Visibility.Hidden;

            tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;

            var taskSystem = Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                foreach (Game game in PlayniteApi.Database.Games)
                {
                    var gameLocalisations = localizationsApi.GetLocalizations(game);

                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        if (gameLocalisations.Count > 0)
                        {
                            CountFind += 1;
                        }
                        else
                        {
                            CountNotFind += 1;
                        }

                        pbDataLoad.Value += 1;
                        tbDataLoad.Text = string.Format(resources.GetString("LOCCheckLocalizationsProgressBar"), CountFind, CountNotFind);
                    }));

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, tokenSource.Token)
            .ContinueWith(antecedent =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    DataLoad.Visibility = Visibility.Collapsed;
                    spSettings.Visibility = Visibility.Visible;
                }));
            });
        }

        /// <summary>
        /// Delete all games localizations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            string PluginDirectory = PluginUserDataPath + "\\CheckLocalizations\\";
            if (Directory.Exists(PluginDirectory))
            {
                try
                {
                    Directory.Delete(PluginDirectory, true);
                    Directory.CreateDirectory(PluginDirectory);
                }
                catch
                {
                    PlayniteApi.Dialogs.ShowErrorMessage(resources.GetString("LOCCheckLocalizationsErrorRemove"), "CheckLocalizations");
                }
            }
        }
    }
}