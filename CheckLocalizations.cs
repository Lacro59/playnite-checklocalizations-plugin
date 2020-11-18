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



        public static LocalizationsDatabase PluginDatabase { get; set; }
        public static List<GameLanguage> GameLanguages = new List<GameLanguage>();

        public static Game GameSelected { get; set; }
        public static CheckLocalizationsUI checkLocalizationsUI;
        public static List<Models.Localization> gameLocalizations { get; set; } = new List<Models.Localization>();

        public CheckLocalizations(IPlayniteAPI api) : base(api)
        {
            settings = new CheckLocalizationsSettings(this);

            // Loading plugin database
            PluginDatabase = new LocalizationsDatabase(PlayniteApi, settings, this.GetPluginUserDataPath());
            PluginDatabase.InitializeDatabase();

            GameLanguages = settings.GameLanguages;

            // Get plugin's location 
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add plugin localization in application ressource.
            PluginCommon.PluginLocalization.SetPluginLanguage(pluginFolder, api.ApplicationSettings.Language);
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

        // To add new game menu items override GetGameMenuItems
        public override List<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var gameMenu = args.Games.First();   

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>
            {
                new GameMenuItem {
                    MenuSection = resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCheckLocalizationsGameMenuPluginView"),
                    Action = (gameMenuItem) =>
                    {
                        var ViewExtension = new CheckLocalizationsView(PluginDatabase.Get(gameMenu.Id));
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "CheckLocalizations", ViewExtension);
                        windowExtension.ShowDialog();
                    }
                },
                new GameMenuItem {
                    MenuSection = resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonRefreshGameData"),
                    Action = (gameMenuItem) =>
                    {
                        PluginDatabase.Remove(gameMenu.Id);
                        var TaskIntegrationUI = Task.Run(() =>
                        {
                            checkLocalizationsUI.RefreshElements(gameMenu);
                        });
                    }
                },

                new GameMenuItem
                {
                    MenuSection = resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCheckLocalizationsGameMenuAddLanguage"),
                    Action = (mainMenuItem) =>
                    {
                        var ViewExtension = new CheckLocalizationsEditManual(gameMenu);
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "CheckLocalizations", ViewExtension);
                        windowExtension.ShowDialog();
                    }
                }
            };

#if DEBUG
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = resources.GetString("LOCCheckLocalizations"),
                Description = "Test",
                Action = (mainMenuItem) => 
                {
                }
            });
#endif

            return gameMenuItems;
        }

        // To add new main menu items override GetMainMenuItems
        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string MenuInExtensions = string.Empty;
            if (settings.MenuInExtensions)
            {
                MenuInExtensions = "@";
            }

            List<MainMenuItem> mainMenuItems = new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonGetAllDatas"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.GetAllDataFromMain();
                    }
                },
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonClearAllDatas"),
                    Action = (mainMenuItem) =>
                    {
                        if (PluginDatabase.ClearDatabase())
                        {
                            PlayniteApi.Dialogs.ShowMessage(resources.GetString("LOCCommonDataRemove"), "CheckLocalizations");
                        }
                        else
                        {
                            PlayniteApi.Dialogs.ShowErrorMessage(resources.GetString("LOCCommonDataErrorRemove"), "CheckLocalizations");
                        }
                    }
                },
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonAddAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddAllTagFromMain();
                    }
                },
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonRemoveAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.RemoveAllTagFromMain();
                    }
                }
            };

#if DEBUG
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                Description = "Test",
                Action = (mainMenuItem) => 
                {
                }
            });
#endif

            return mainMenuItems;
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            try
            {
                if (args.NewValue != null && args.NewValue.Count == 1)
                {
                    GameSelected = args.NewValue[0];

                    //PlayniteUiHelper.ResetToggle();
                    var TaskIntegrationUI = Task.Run(() =>
                    {
                        checkLocalizationsUI.Initial();
                        checkLocalizationsUI.taskHelper.Check();
                        var dispatcherOp = checkLocalizationsUI.AddElements();
                        dispatcherOp.Completed += (s, e) => { checkLocalizationsUI.RefreshElements(GameSelected); };
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
            return new CheckLocalizationsSettingsView(PlayniteApi, settings, this.GetPluginUserDataPath());
        }
    }
}