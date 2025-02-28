using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using CheckLocalizations.Views;
using CheckLocalizations.Services;
using System.Windows;
using CheckLocalizations.Models;
using CommonPluginsShared;
using CommonPluginsShared.PlayniteExtended;
using CheckLocalizations.Controls;
using CommonPluginsControls.Views;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLocalizations
{
    public class CheckLocalizations : PluginExtended<CheckLocalizationsSettingsViewModel, LocalizationsDatabase>
    {
        public override Guid Id { get; } = Guid.Parse("7ce83cfe-7894-4ad9-957d-7249c0fb3e7d");

        private bool PreventLibraryUpdatedOnStart { get; set; } = true;


        public CheckLocalizations(IPlayniteAPI api) : base(api)
        {
            // Custom theme button
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnCustomThemeButtonClick));

            // Custom elements integration
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "PluginButton", "PluginViewItem", "PluginListLanguages", "PluginFlags" },
                SourceName = PluginDatabase.PluginName
            });

            // Settings integration
            AddSettingsSupport(new AddSettingsSupportArgs
            {
                SourceName = PluginDatabase.PluginName,
                SettingsRoot = $"{nameof(PluginSettings)}.{nameof(PluginSettings.Settings)}"
            });
        }


        #region Custom event
        public void OnCustomThemeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string ButtonName = ((Button)sender).Name;
                if (ButtonName == "PART_CustomCheckLocButton")
                {
                    CheckLocalizationsView ViewExtension = new CheckLocalizationsView();
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginDatabase.PluginName, ViewExtension);
                    windowExtension.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }
        #endregion


        #region Theme integration
        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            yield break;
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

            if (args.Name == "PluginFlags")
            {
                return new PluginFlags();
            }

            return null;
        }
        #endregion


        #region Menus
        // Add new game menu items override GetGameMenuItems
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game gameMenu = args.Games.First();
            List<Guid> ids = args.Games.Select(x => x.Id).ToList();
            GameLocalizations gameLocalizations = PluginDatabase.Get(gameMenu, true);

            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            if (gameLocalizations.HasData)
            {
                // Show list available localizations for the selected game
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCheckLocalizationsGameMenuPluginView"),
                    Action = (gameMenuItem) =>
                    {
                        CheckLocalizationsView ViewExtension = new CheckLocalizationsView();
                        Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginDatabase.PluginName, ViewExtension);
                        windowExtension.ShowDialog();
                    }
                });

                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = "-"
                });
            }


            // Download localizations data for the selected game
            if (PluginSettings.Settings.UpdateWhenHasManual || !gameLocalizations.HasManual())
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCommonRefreshGameData"),
                    Action = (gameMenuItem) =>
                    {
                        if (ids.Count == 1)
                        {
                            PluginDatabase.Refresh(gameMenu.Id);
                        }
                        else
                        {
                            PluginDatabase.Refresh(ids);
                        }
                    }
                });
            }

            // Open editor view to add a new supported language for the selected game
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = ResourceProvider.GetString("LOCCheckLocalizationsGameMenuAddLanguage"),
                Action = (mainMenuItem) =>
                {
                    CheckLocalizationsEditManual ViewExtension = new CheckLocalizationsEditManual(gameMenu);
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginDatabase.PluginName, ViewExtension);
                    windowExtension.ShowDialog();
                }
            });


            if (gameLocalizations.HasData)
            {
                gameMenuItems.Add(new GameMenuItem
                {
                    MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCommonDeleteGameData"),
                    Action = (mainMenuItem) =>
                    {
                        _ = ids.Count == 1 ? PluginDatabase.RemoveWithManual(gameMenu.Id) : PluginDatabase.RemoveWithManual(ids);
                    }
                });
            }

#if DEBUG
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = "-"
            });
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = "Test",
                Action = (mainMenuItem) =>
                {

                }
            });
#endif

            return gameMenuItems;
        }

        // Add new main menu items override GetMainMenuItems
        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
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
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCommonDownloadPluginData"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.GetSelectData();
                    }
                }
            };


            if (PluginDatabase.PluginSettings.Settings.EnableTag)
            {
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = "-"
                });

                // Add tag for selected game in database if data exists
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCommonAddTPlugin"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddTagSelectData();
                    }
                });
                // Add tag for all games
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCommonAddAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.AddTagAllGame();
                    }
                });
                // Remove tag for all game in database
                mainMenuItems.Add(new MainMenuItem
                {
                    MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                    Description = ResourceProvider.GetString("LOCCommonRemoveAllTags"),
                    Action = (mainMenuItem) =>
                    {
                        PluginDatabase.RemoveTagAllGame();
                    }
                });
            }


            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = "-"
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = "LOCCommonViewNoData",
                Action = (mainMenuItem) =>
                {
                    WindowOptions windowOptions = new WindowOptions
                    {
                        ShowMinimizeButton = false,
                        ShowMaximizeButton = false,
                        ShowCloseButton = true
                    };

                    ListWithNoData ViewExtension = new ListWithNoData(PluginDatabase);
                    Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCheckLocalizations"), ViewExtension, windowOptions);
                    windowExtension.ShowDialog();
                }
            });


            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = "-"
            });

            // Delete all data of plugin
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = ResourceProvider.GetString("LOCCommonDeletePluginData"),
                Action = (mainMenuItem) =>
                {
                    _ = PluginDatabase.ClearDatabase()
                        ? PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCCommonDataRemove"), PluginDatabase.PluginName)
                        : PlayniteApi.Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCCommonDataErrorRemove"), PluginDatabase.PluginName);
                }
            });

#if DEBUG
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
                Description = "-"
            });
            mainMenuItems.Add(new MainMenuItem
            {
                MenuSection = MenuInExtensions + ResourceProvider.GetString("LOCCheckLocalizations"),
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
        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            try
            {
                if (args.NewValue?.Count == 1)
                {
                    PluginDatabase.GameContext = args.NewValue[0];
                    PluginDatabase.SetThemesResources(PluginDatabase.GameContext);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }

        // Add code to be executed when game is finished installing.
        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {

        }

        // Add code to be executed when game is uninstalled.
        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStarting(OnGameStartingEventArgs args)
        {

        }

        // Add code to be executed when game is started running.
        public override void OnGameStarted(OnGameStartedEventArgs args)
        {

        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {

        }
        #endregion


        #region Application event
        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            _ = Task.Run(() =>
            {
                Thread.Sleep(10000);
                PreventLibraryUpdatedOnStart = false;
            });
        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {

        }
        #endregion


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            if (PluginSettings.Settings.AutoImport && !PreventLibraryUpdatedOnStart)
            {
                PluginDatabase.RefreshRecent();
                PluginSettings.Settings.LastAutoLibUpdateAssetsDownload = DateTime.Now;
                SavePluginSettings(PluginSettings.Settings);
            }
        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckLocalizationsSettingsView(PluginSettings.Settings);
        }
        #endregion
    }
}
