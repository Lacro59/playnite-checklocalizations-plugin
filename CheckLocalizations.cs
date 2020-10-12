using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.API;
using PluginCommon.PlayniteResources.Common;
using PluginCommon.PlayniteResources.Converters;
using CheckLocalizations.Views;
using CheckLocalizations.Services;
using Playnite.SDK.Events;
using Newtonsoft.Json;
using System.Windows;
using CheckLocalizations.Models;

namespace CheckLocalizations
{
    public class CheckLocalizations : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private CheckLocalizationsSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("7ce83cfe-7894-4ad9-957d-7249c0fb3e7d");

        public static Game GameSelected { get; set; }
        public static CheckLocalizationsUI checkLocalizationsUI;
        public static List<GameLocalization> gameLocalizations { get; set; } = new List<GameLocalization>();


        public CheckLocalizations(IPlayniteAPI api) : base(api)
        {
            settings = new CheckLocalizationsSettings(this);

            // Get plugin's location 
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add plugin localization in application ressource.
            PluginCommon.Localization.SetPluginLanguage(pluginFolder, api.ApplicationSettings.Language);
            // Add common in application ressource.
            PluginCommon.Common.Load(pluginFolder);

            // Check version
            if (settings.EnableCheckVersion)
            {
                CheckVersion cv = new CheckVersion();
                if (cv.Check("CheckLocalizations", pluginFolder))
                {
                    cv.ShowNotification(api, "CheckLocalizations - " + resources.GetString("LOCUpdaterWindowTitle"));
                }
            }

            // Init ui interagration
            checkLocalizationsUI = new CheckLocalizationsUI(api, settings, this.GetPluginUserDataPath());

            // Custom theme button
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(checkLocalizationsUI.OnCustomThemeButtonClick));
        }

        public override IEnumerable<ExtensionFunction> GetFunctions()
        {
            List<ExtensionFunction> listFunctions = new List<ExtensionFunction>();

            listFunctions.Add(
                new ExtensionFunction(
                    resources.GetString("LOCCheckLocalizations"),
                    () =>
                    {
                        var ViewExtension = new CheckLocalizationsView(gameLocalizations);
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "CheckLocalizations", ViewExtension);
                        windowExtension.ShowDialog();
                    })
                );

#if DEBUG
            listFunctions.Add(
                new ExtensionFunction(
                    "CheckLocalizations Test",
                    () =>
                    {

                    })
                );
#endif

            return listFunctions;
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            try
            {
                if (args.NewValue != null && args.NewValue.Count == 1)
                {
                    GameSelected = args.NewValue[0];

                    var TaskIntegrationUI = Task.Run(() =>
                    {
                        checkLocalizationsUI.AddElements();
                        checkLocalizationsUI.RefreshElements(GameSelected);
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", $"OnGameSelected()");
            }
        }

        public override void OnGameInstalled(Game game)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(Game game)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(Game game)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(Game game, long elapsedSeconds)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(Game game)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted()
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped()
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated()
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckLocalizationsSettingsView(this.GetPluginUserDataPath(), PlayniteApi, settings);
        }
    }
}