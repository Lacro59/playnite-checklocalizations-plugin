using CheckLocalizations.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using CommonPluginsShared.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonPluginsShared;
using System.Windows;
using System.Windows.Threading;

namespace CheckLocalizations.Services
{
    public class LocalizationsDatabase : PluginDatabaseObject<CheckLocalizationsSettingsViewModel, GameLocalizationsCollection, GameLocalizations, Models.Localization>
    {
        private LocalizationsApi localizationsApi;
        private bool IsGetWeb = false;


        public LocalizationsDatabase(IPlayniteAPI PlayniteApi, CheckLocalizationsSettingsViewModel PluginSettings, string PluginUserDataPath) : base(PlayniteApi, PluginSettings, "CheckLocalizations", PluginUserDataPath)
        {
            TagBefore = "[CL]";
        }
        

        protected override bool LoadDatabase()
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                Database = new GameLocalizationsCollection(Paths.PluginDatabasePath);
                Database.SetGameInfo<Models.Localization>(PlayniteApi);

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"LoadDatabase with {Database.Count} items - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "CheckLocalizations");
                return false;
            }

            return true;
        }


        public override GameLocalizations Get(Guid Id, bool OnlyCache = false, bool Force = false)
        {
            GameLocalizations gameLocalizations = GetOnlyCache(Id);

            if ((gameLocalizations == null && !OnlyCache) || Force)
            {
                gameLocalizations = GetWeb(Id);
                AddOrUpdate(gameLocalizations);
            }
            else if (gameLocalizations != null && !OnlyCache
                && gameLocalizations.Items.Where(x => x.IsManual == true).Count() != 0
                && gameLocalizations.Items.Where(x => x.IsManual == false).Count() == 0)
            {
                if (!gameLocalizations.HasChecked)
                {
                    var dataWeb = GetWeb(Id);

                    gameLocalizations.Items = gameLocalizations.Items.Concat(dataWeb.Items).ToList();
                    gameLocalizations.HasChecked = true;

                    Update(gameLocalizations);
                }
            }
            else if (gameLocalizations == null)
            {
                Game game = PlayniteApi.Database.Games.Get(Id);
                if (game != null)
                {
                    gameLocalizations = GetDefault(game);
                    Add(gameLocalizations);
                }
            }

            gameLocalizations.Items.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            return gameLocalizations;
        }

        public override GameLocalizations GetWeb(Guid Id)
        {
            IsGetWeb = true;

            if (localizationsApi == null)
            {
                localizationsApi = new LocalizationsApi(PlayniteApi, Paths.PluginUserDataPath);
            }

            var data = localizationsApi.GetLocalizations(Id);

            Task.Run(() =>
            {
                Thread.Sleep(2000);

                if (!IsGetWeb)
                {
                    localizationsApi.Dispose();
                    localizationsApi = null;
                    GC.Collect();
                }
            });

            IsGetWeb = false;
            
            return data;
        }


        public override void Refresh(Guid Id)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"{PluginName} - {resources.GetString("LOCCommonProcessing")}",
                false
            );
            globalProgressOptions.IsIndeterminate = true;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                var loadedItem = Get(Id, true);
                var webItem = GetWeb(Id);

                if (webItem != null)
                {
                    // Add manual items
                    foreach (var item in loadedItem.Items.FindAll(x => x.IsManual))
                    {
                        webItem.Items.Add(item);
                    }

                    if (!ReferenceEquals(loadedItem, webItem))
                    {
                        Update(webItem);
                    }
                }
            }, globalProgressOptions);
        }

        public override void Refresh(List<Guid> Ids)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"{PluginName} - {resources.GetString("LOCCommonProcessing")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                activateGlobalProgress.ProgressMaxValue = Ids.Count;

                string CancelText = string.Empty;

                foreach (Guid Id in Ids)
                {
                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                    {
                        CancelText = " canceled";
                        break;
                    }

                    var loadedItem = Get(Id, true);
                    var webItem = GetWeb(Id);

                    if (webItem != null)
                    {
                        // Add manual items
                        foreach (var item in loadedItem.Items.FindAll(x => x.IsManual))
                        {
                            webItem.Items.Add(item);
                        }

                        if (!ReferenceEquals(loadedItem, webItem))
                        {
                            Update(webItem);
                        }
                    }
                    
                    activateGlobalProgress.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"Task Refresh(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{Ids.Count} items");
            }, globalProgressOptions);
        }


        public bool RemoveWithManual(Guid Id)
        {
            try
            {
                GameLocalizations gameLocalizations = GetOnlyCache(Id);

                if (gameLocalizations.Items == null)
                {
                    gameLocalizations.Items = new List<Models.Localization>();
                }


                if (gameLocalizations.Items.Where(x => x.IsManual).Count() == 0)
                {
                    return Remove(Id);
                }
                else
                {
                    var ItemsManual = gameLocalizations.Items.Where(x => x.IsManual).ToList();
                    gameLocalizations.Items = null;
                    gameLocalizations.Items = ItemsManual;
                    gameLocalizations.HasChecked = false;

                    Common.LogDebug(true, $"RemoveWithoutManual({Id.ToString()}) - gameLocalizations: {Serialization.ToJson(gameLocalizations)}");

                    Update(gameLocalizations);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "CheckLocalizations");
            }

            return false;
        }

        public bool RemoveWithManual(List<Guid> Ids)
        {
            foreach (Guid Id in Ids)
            {
                try
                {
                    GameLocalizations gameLocalizations = GetOnlyCache(Id);

                    if (gameLocalizations == null || !gameLocalizations.HasData)
                    {
                        continue;
                    }

                    if (gameLocalizations.Items == null)
                    {
                        gameLocalizations.Items = new List<Models.Localization>();
                    }


                    if (gameLocalizations.Items.Where(x => x.IsManual).Count() == 0)
                    {
                        Remove(Id);
                    }
                    else
                    {
                        var ItemsManual = gameLocalizations.Items.Where(x => x.IsManual).ToList();
                        gameLocalizations.Items = null;
                        gameLocalizations.Items = ItemsManual;
                        gameLocalizations.HasChecked = false;

                        Common.LogDebug(true, $"RemoveWithoutManual({Id.ToString()}) - gameLocalizations: {Serialization.ToJson(gameLocalizations)}");

                        Update(gameLocalizations);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "CheckLocalizations");
                }
            }

            return true;
        }


        #region Tag
        public override void AddTag(Game game, bool noUpdate = false)
        {
            GetPluginTags();
            GameLocalizations gameLocalizations = Get(game, true);

            if (gameLocalizations.HasData)
            {
                try
                {
                    foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages.FindAll(x => x.IsTag && gameLocalizations.Items.Any(y => x.Name.ToLower() == y.Language.ToLower())))
                    {
                        Guid? TagId = FindGoodPluginTags(gameLanguage.DisplayName);
                        if (TagId != null)
                        {
                            if (game.TagIds != null)
                            {
                                if (!game.TagIds.Contains((Guid)TagId))
                                {
                                    game.TagIds.Add((Guid)TagId);
                                }
                            }
                            else
                            {
                                game.TagIds = new List<Guid> { (Guid)TagId };
                            }
                        }
                    }

                    if (!noUpdate)
                    {
                        Application.Current.Dispatcher?.Invoke(() => 
                        {
                            PlayniteApi.Database.Games.Update(game);
                            game.OnPropertyChanged();
                        }, DispatcherPriority.Send);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, true);
                    logger.Error($"Tag insert error with {game.Name}");

                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        $"{PluginName}-Tag-Errors",
                        $"{PluginName}\r\n" + resources.GetString("LOCCommonNotificationTagError"),
                        NotificationType.Error
                    ));
                }
            }
        }
        #endregion


        public override void SetThemesResources(Game game)
        {
            GameLocalizations gameLocalizations = Get(game, true);

            if (gameLocalizations == null)
            {
                PluginSettings.Settings.HasData = false;
                PluginSettings.Settings.HasNativeSupport = false;
                PluginSettings.Settings.ListNativeSupport = new List<Models.Localization>();

                return;
            }

            PluginSettings.Settings.HasData = gameLocalizations.HasData;
            PluginSettings.Settings.HasNativeSupport = gameLocalizations.HasNativeSupport();
            PluginSettings.Settings.ListNativeSupport = gameLocalizations.Items;
        }

        public override void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            foreach (var GameUpdated in e.UpdatedItems)
            {
                Database.SetGameInfo<Models.Localization>(PlayniteApi, GameUpdated.NewData.Id);
            }
        }
    }
}
