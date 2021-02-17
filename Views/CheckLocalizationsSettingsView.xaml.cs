using CheckLocalizations.Models;
using CheckLocalizations.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CheckLocalizations.Views
{
    public partial class CheckLocalizationsSettingsView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private IPlayniteAPI PlayniteApi { get; set; }
        private CheckLocalizationsSettings settings { get; set; }
        private string PluginUserDataPath { get; set; }


        public CheckLocalizationsSettingsView(IPlayniteAPI PlayniteApi, CheckLocalizationsSettings settings, string PluginUserDataPath)
        {
            this.PlayniteApi = PlayniteApi;
            this.settings = settings;
            this.PluginUserDataPath = PluginUserDataPath;

            InitializeComponent();

            lbGameLanguages.ItemsSource = settings.GameLanguages.OrderBy(x => x.Name).ToList();

            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            imgNative.Source = new BitmapImage(new Uri($"{pluginFolder}\\Resources\\native.png"));
            imgNoNative.Source = new BitmapImage(new Uri($"{pluginFolder}\\Resources\\nonative.png"));
            imgNotApplicable.Source = new BitmapImage(new Uri($"{pluginFolder}\\Resources\\notapplicable.png"));
            imgUnknown.Source = new BitmapImage(new Uri($"{pluginFolder}\\Resources\\unknown.png"));
        }

        /// <summary>
        /// Remove tag localizations in all games
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            CheckLocalizations.PluginDatabase.AddTagAllGame();
        }

        /// <summary>
        /// Remove tag localizations in all games
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            CheckLocalizations.PluginDatabase.RemoveTagAllGame();
        }


        /// <summary>
        /// Get all games localizations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckLocalizations.PluginDatabase.GetAllDatas();
        }

        /// <summary>
        /// Delete all games localizations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            if (CheckLocalizations.PluginDatabase.ClearDatabase())
            {
                PlayniteApi.Dialogs.ShowMessage(resources.GetString("LOCCommonDataRemove"), "CheckLocalizations");
            }
            else
            {
                PlayniteApi.Dialogs.ShowErrorMessage(resources.GetString("LOCCommonDataErrorRemove"), "CheckLocalizations");
            }
        }


        private void RbSteam_Click(object sender, RoutedEventArgs e)
        {
            rbPcGamingWiki.IsChecked = !rbSteam.IsChecked;
        }

        private void RbPcGamingWiki_Click(object sender, RoutedEventArgs e)
        {
            rbSteam.IsChecked = !rbPcGamingWiki.IsChecked;
        }
    }
}