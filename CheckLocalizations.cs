﻿using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using CheckLocalizations.Views;
using CheckLocalizations.Services;
using System.Windows;
using CheckLocalizations.Models;
using CommonPluginsShared;
using CommonPluginsShared.PlayniteExtended;

namespace CheckLocalizations
{
    public class CheckLocalizations : PluginExtended<CheckLocalizationsSettingsViewModel, LocalizationsDatabase>
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        public override Guid Id { get; } = Guid.Parse("7ce83cfe-7894-4ad9-957d-7249c0fb3e7d");


        public static CheckLocalizationsUI checkLocalizationsUI;

        private OldToNew oldToNew;


        public CheckLocalizations(IPlayniteAPI api) : base(api, true)
        {
            // Old database
            oldToNew = new OldToNew(this.GetPluginUserDataPath());


            // Init ui interagration
            checkLocalizationsUI = new CheckLocalizationsUI(api, this.GetPluginUserDataPath());

            // Custom theme button
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(checkLocalizationsUI.OnCustomThemeButtonClick));

            // Add event fullScreen
            if (api.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(BtFullScreen_ClickEvent));
            }



            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "TestUserControl", "TestUserControl2" },
                SourceName = "CheckLocalizations",
                SettingsRoot = $"{nameof(PluginSettings)}.{nameof(PluginSettings.Settings)}"
            });
        }


        #region Custom event
        private void BtFullScreen_ClickEvent(object sender, System.EventArgs e)
        {
            try
            {
                if (((Button)sender).Name == "PART_ButtonDetails")
                {
                    var TaskIntegrationUI = Task.Run(() =>
                    {
                        checkLocalizationsUI.Initial();
                        checkLocalizationsUI.taskHelper.Check();
                        var dispatcherOp = checkLocalizationsUI.AddElementsFS();
                        dispatcherOp.Completed += (s, ev) => { checkLocalizationsUI.RefreshElements(LocalizationsDatabase.GameSelected); };
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }
        #endregion



        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "TestUserControl")
            {
                //return new SuccessStoryAchievementsList(false, PlayniteApi);
            }

            return null;
        }



        // Add new game menu items override GetGameMenuItems
        public override List<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game GameMenu = args.Games.First();   

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>
            {
                // Show list available localizations for the selected game
                new GameMenuItem {
                    MenuSection = resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCheckLocalizationsGameMenuPluginView"),
                    Action = (gameMenuItem) =>
                    {
                        var ViewExtension = new CheckLocalizationsView();
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "CheckLocalizations", ViewExtension);
                        windowExtension.ShowDialog();
                    }
                },

                // Delete & download localizations data for the selected game
                new GameMenuItem {
                    MenuSection = resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonRefreshGameData"),
                    Action = (gameMenuItem) =>
                    {
                        PluginDatabase.GameSelectedData = new GameLocalizations();
                        var TaskIntegrationUI = Task.Run(() =>
                        {
                            PluginDatabase.RemoveWithManual(GameMenu.Id);
                            checkLocalizationsUI.RefreshElements(GameMenu);
                        });
                    }
                },

                // Open editor view to add a new supported language for the selected game
                new GameMenuItem
                {
                    MenuSection = resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCheckLocalizationsGameMenuAddLanguage"),
                    Action = (mainMenuItem) =>
                    {
                        var ViewExtension = new CheckLocalizationsEditManual();
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

        // Add new main menu items override GetMainMenuItems
        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string MenuInExtensions = string.Empty;
            if (PluginSettings.Settings.MenuInExtensions)
            {
                MenuInExtensions = "@";
            }

            List<MainMenuItem> mainMenuItems = new List<MainMenuItem>
            {
                // Download missing localizations data for all game in database
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonGetAllDatas"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.GetAllDatas();
                    }
                },

                // Delete all data of plugin
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

                // Add tag for all game in database if data exists
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonAddAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddTagAllGame();
                    }
                },

                // Remove tag for all game in database
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonRemoveAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.RemoveTagAllGame();
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
            // Old database
            if (oldToNew.IsOld)
            {
                oldToNew.ConvertDB(PlayniteApi);
            }

            try
            {
                if (args.NewValue != null && args.NewValue.Count == 1)
                {
                    Game GameSelected = args.NewValue[0];

                    GameLocalizations gameLocalizations = PluginDatabase.Get(GameSelected, true);

                    PluginSettings.Settings.TestString = GameSelected.Name;

                    PluginSettings.Settings.HasData = gameLocalizations.HasData;
                    PluginSettings.Settings.HasNativeSupport = gameLocalizations.HasNativeSupport();
                    PluginSettings.Settings.ListNativeSupport = gameLocalizations.Items;



                    // Old system
                    /*
                    LocalizationsDatabase.GameSelected = GameSelected;

                    var TaskIntegrationUI = Task.Run(() =>
                    {
                        checkLocalizationsUI.Initial();
                        checkLocalizationsUI.taskHelper.Check();
                        var dispatcherOp = checkLocalizationsUI.AddElements();
                        if (dispatcherOp != null)
                        {
                            dispatcherOp.Completed += (s, e) => { checkLocalizationsUI.RefreshElements(GameSelected); };
                        }
                    });
                    */
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }

        // Add code to be executed when game is finished installing.
        public override void OnGameInstalled(Game game)
        {

        }

        // Add code to be executed when game is started running.
        public override void OnGameStarted(Game game)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStarting(Game game)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStopped(Game game, long elapsedSeconds)
        {

        }

        // Add code to be executed when game is uninstalled.
        public override void OnGameUninstalled(Game game)
        {
            
        }


        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted()
        {

        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped()
        {

        }


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated()
        {

        }


        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckLocalizationsSettingsView(PlayniteApi, PluginSettings.Settings, this.GetPluginUserDataPath());
        }
    }
}
