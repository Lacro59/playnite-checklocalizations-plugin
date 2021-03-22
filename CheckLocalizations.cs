using Playnite.SDK;
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
using System.Windows.Media;
using System.Diagnostics;
using CheckLocalizations.Controls;

namespace CheckLocalizations
{
    public class CheckLocalizations : PluginExtended<CheckLocalizationsSettingsViewModel, LocalizationsDatabase>
    {
        public override Guid Id { get; } = Guid.Parse("7ce83cfe-7894-4ad9-957d-7249c0fb3e7d");

        private OldToNew oldToNew;


        public CheckLocalizations(IPlayniteAPI api) : base(api)
        {
            // Old database
            oldToNew = new OldToNew(this.GetPluginUserDataPath());

            // Custom theme button
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnCustomThemeButtonClick));

            // Custom elements integration
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "PluginButton", "PluginViewItem", "PluginListLanguages" },
                SourceName = "CheckLocalizations",
                SettingsRoot = $"{nameof(PluginSettings)}.{nameof(PluginSettings.Settings)}"
            });
        }


        #region Custom event
        public void OnCustomThemeButtonClick(object sender, RoutedEventArgs e)
        {
            string ButtonName = string.Empty;
            try
            {
                ButtonName = ((Button)sender).Name;
                if (ButtonName == "PART_CustomCheckLocButton")
                {
                    var ViewExtension = new CheckLocalizationsView();
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, "CheckLocalizations", ViewExtension);
                    windowExtension.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }
        #endregion


        #region Theme integration
        public override List<TopPanelItem> GetTopPanelItems()
        {
            return null;
        }

        // List custom controls
        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "PluginButton")
            {
                return new PluginButton();
            }

            if (args.Name == "PluginViewItem")
            {
                return new PluginViewItem();
            }

            if (args.Name == "PluginListLanguages")
            {
                return new PluginListLanguages();
            }

            return null;
        }
        #endregion


        #region Menus
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
                        var TaskIntegrationUI = Task.Run(() =>
                        {
                            PluginDatabase.Refresh(GameMenu.Id);
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
                        var ViewExtension = new CheckLocalizationsEditManual(GameMenu);
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
                // Download missing localizations data for selected games in database
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonDownloadPluginData"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.GetSelectData();
                    }
                },

                // Add tag for selected game in database if data exists
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonAddTPlugin"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddTagSelectData();
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
                },

                // Delete all data of plugin
                new MainMenuItem
                {
                    MenuSection = MenuInExtensions + resources.GetString("LOCCheckLocalizations"),
                    Description = resources.GetString("LOCCommonDeletePluginData"),
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
        #endregion


        #region Game event
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
                    PluginDatabase.GameContext = args.NewValue[0];
                    PluginDatabase.SetThemesResources(PluginDatabase.GameContext);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
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
        #endregion


        #region Application event
        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted()
        {

        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped()
        {

        }
        #endregion


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated()
        {

        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckLocalizationsSettingsView(PlayniteApi, PluginSettings.Settings, this.GetPluginUserDataPath());
        }
        #endregion
    }
}
