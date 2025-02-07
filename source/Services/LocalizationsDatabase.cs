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
        private LocalizationsApi LocalizationsApi { get; set; }
        private bool IsGetWeb { get; set; } = false;


        public LocalizationsDatabase(CheckLocalizationsSettingsViewModel pluginSettings, string pluginUserDataPath) : base(pluginSettings, "CheckLocalizations", pluginUserDataPath)
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
                Database.SetGameInfo<Models.Localization>();

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Logger.Info($"LoadDatabase with {Database.Count} items - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return false;
            }

            return true;
        }


        public override GameLocalizations Get(Guid id, bool onlyCache = false, bool force = false)
        {
            GameLocalizations gameLocalizations = GetOnlyCache(id);

            if ((gameLocalizations == null && !onlyCache) || force)
            {
                gameLocalizations = GetWeb(id);
                AddOrUpdate(gameLocalizations);
            }
            else if (gameLocalizations != null && !onlyCache
                && gameLocalizations.Items.Where(x => x.IsManual == true).Count() != 0
                && gameLocalizations.Items.Where(x => x.IsManual == false).Count() == 0)
            {
                if (!gameLocalizations.HasChecked)
                {
                    var dataWeb = GetWeb(id);

                    gameLocalizations.Items = gameLocalizations.Items.Concat(dataWeb.Items).ToList();
                    gameLocalizations.HasChecked = true;

                    Update(gameLocalizations);
                }
            }
            else if (gameLocalizations == null)
            {
                Game game = API.Instance.Database.Games.Get(id);
                if (game != null)
                {
                    gameLocalizations = GetDefault(game);
                    Add(gameLocalizations);
                }
            }

            gameLocalizations?.Items?.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            return gameLocalizations;
        }

        public override GameLocalizations GetWeb(Guid id)
        {
            IsGetWeb = true;

            if (LocalizationsApi == null)
            {
                LocalizationsApi = new LocalizationsApi();
            }

            GameLocalizations data = LocalizationsApi.GetLocalizations(id);

            Task.Run(() =>
            {
                Thread.Sleep(2000);

                if (!IsGetWeb)
                {
                    LocalizationsApi.Dispose();
                    LocalizationsApi = null;
                    GC.Collect();
                }
            });

            IsGetWeb = false;

            return data;
        }


        public override void RefreshNoLoader(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            Logger.Info($"RefreshNoLoader({game?.Name} - {game?.Id})");

            GameLocalizations loadedItem = Get(id, true);
            GameLocalizations webItem = GetWeb(id);

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

            ActionAfterRefresh(webItem);
        }


        public bool RemoveWithManual(Guid id)
        {
            try
            {
                GameLocalizations gameLocalizations = GetOnlyCache(id);

                if (gameLocalizations.Items == null)
                {
                    gameLocalizations.Items = new List<Models.Localization>();
                }


                if (gameLocalizations.Items.Where(x => x.IsManual).Count() == 0)
                {
                    return Remove(id);
                }
                else
                {
                    List<Models.Localization> itemsManual = gameLocalizations.Items.Where(x => x.IsManual).ToList();
                    gameLocalizations.Items = null;
                    gameLocalizations.Items = itemsManual;
                    gameLocalizations.HasChecked = false;

                    Common.LogDebug(true, $"RemoveWithoutManual({id}) - gameLocalizations: {Serialization.ToJson(gameLocalizations)}");

                    Update(gameLocalizations);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return false;
        }

        public bool RemoveWithManual(List<Guid> ids)
        {
            foreach (Guid id in ids)
            {
                try
                {
                    GameLocalizations gameLocalizations = GetOnlyCache(id);

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
                        Remove(id);
                    }
                    else
                    {
                        List<Models.Localization> ItemsManual = gameLocalizations.Items.Where(x => x.IsManual).ToList();
                        gameLocalizations.Items = null;
                        gameLocalizations.Items = ItemsManual;
                        gameLocalizations.HasChecked = false;

                        Common.LogDebug(true, $"RemoveWithoutManual({id}) - gameLocalizations: {Serialization.ToJson(gameLocalizations)}");

                        Update(gameLocalizations);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return true;
        }


        #region Tag
        public override void AddTag(Game game)
        {
            GameLocalizations item = Get(game, true);
            if (item.HasData)
            {
                try
                {
                    if (PluginSettings.Settings.EnableTagSingle)
                    {
                        foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages.FindAll(x => x.IsTag && item.Items.Any(y => x.Name.ToLower() == y.Language.ToLower())))
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
                    }

                    //🔈
                    if (PluginSettings.Settings.EnableTagAudio)
                    {
                        foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages.FindAll(x => x.IsTag && item.Items.Any(y => x.Name.ToLower() == y.Language.ToLower() && y.IsOkAudio)))
                        {
                            Guid? TagId = FindGoodPluginTags("🔈" + gameLanguage.DisplayName);
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
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Tag insert error with {game.Name}", true, PluginName, string.Format(ResourceProvider.GetString("LOCCommonNotificationTagError"), game.Name));
                    return;
                }
            }
            else if (TagMissing)
            {
                if (game.TagIds != null)
                {
                    game.TagIds.Add((Guid)AddNoDataTag());
                }
                else
                {
                    game.TagIds = new List<Guid> { (Guid)AddNoDataTag() };
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() =>
            {
                API.Instance.Database.Games.Update(game);
                game.OnPropertyChanged();
            });
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
    }
}
