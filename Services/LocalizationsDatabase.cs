using CheckLocalizations.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPluginsShared.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonPluginsShared;

namespace CheckLocalizations.Services
{
    public class LocalizationsDatabase : PluginDatabaseObject<CheckLocalizationsSettingsViewModel, GameLocalizationsCollection, GameLocalizations>
    {
        private LocalizationsApi localizationsApi;


        public LocalizationsDatabase(IPlayniteAPI PlayniteApi, CheckLocalizationsSettingsViewModel PluginSettings, string PluginUserDataPath) : base(PlayniteApi, PluginSettings, "CheckLocalizations", PluginUserDataPath)
        {
            localizationsApi = new LocalizationsApi(PlayniteApi, PluginUserDataPath);
        }

        protected override bool LoadDatabase()
        {
            Database = new GameLocalizationsCollection(Paths.PluginDatabasePath);
            Database.SetGameInfo<Localization>(PlayniteApi);
            GetPluginTags();
            return true;
        }


        public override GameLocalizations Get(Guid Id, bool OnlyCache = false)
        {
            GameLocalizations gameLocalizations = GetOnlyCache(Id);
#if DEBUG
            logger.Debug($"{PluginName} [Ignored] - GetFromDb({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
            if (gameLocalizations == null && !OnlyCache)
            {
                gameLocalizations = GetWeb(Id);
                Add(gameLocalizations);

#if DEBUG
                logger.Debug($"{PluginName} [Ignored] - GetFromWeb({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
            }
            else if (gameLocalizations != null && !OnlyCache
                && gameLocalizations.Items.Where(x => x.IsManual == true).Count() != 0
                && gameLocalizations.Items.Where(x => x.IsManual == false).Count() == 0)
            {
                if (!gameLocalizations.HasChecked)
                {
                    var dataWeb = localizationsApi.GetLocalizations(Id);
#if DEBUG
                    logger.Debug($"{PluginName} [Ignored] - GetFromWebOnlyManual({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(dataWeb)}");
                    logger.Debug($"{PluginName} [Ignored] - IsManualTrue({gameLocalizations.Items.Where(x => x.IsManual == true).Count()}) - IsManualFalse: {gameLocalizations.Items.Where(x => x.IsManual == false).Count()}");
#endif
                    gameLocalizations.Items = gameLocalizations.Items.Concat(dataWeb.Items).ToList();
                    gameLocalizations.HasChecked = true;

                    Update(gameLocalizations);
                }
            }
            else if (gameLocalizations == null)
            {
                Game game = PlayniteApi.Database.Games.Get(Id);
                gameLocalizations = GetDefault(game);
                Add(gameLocalizations);
            }

            gameLocalizations.Items.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            return gameLocalizations;
        }

        public override GameLocalizations GetWeb(Guid Id)
        {
            return localizationsApi.GetLocalizations(Id);
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

                // Add manual items
                foreach (var item in loadedItem.Items.FindAll(x => x.IsManual))
                {
                    webItem.Items.Add(item);
                }

                if (!ReferenceEquals(loadedItem, webItem))
                {
                    Update(webItem);
                }
            }, globalProgressOptions);
        }


        public bool RemoveWithManual(Guid Id)
        {
            try
            {
                GameLocalizations gameLocalizations = GetOnlyCache(Id);

                if (gameLocalizations.Items == null)
                {
                    gameLocalizations.Items = new List<Localization>();
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
#if DEBUG
                    logger.Debug($"{PluginName} [Ignored] - RemoveWithoutManual({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
                    Update(gameLocalizations);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, PluginName);
            }

            return false;
        }


        protected override void GetPluginTags()
        {
#if DEBUG
            logger.Debug($"{PluginName} [Ignored] - GetPluginTags()");
#endif
            try
            {
                // Get tags in playnite database
                PluginTags = new List<Tag>();
                foreach (Tag tag in PlayniteApi.Database.Tags)
                {
                    if (tag.Name.IndexOf("[CL] ") > -1)
                    {
                        PluginTags.Add(tag);
                    }
                }

                // Add missing tags
                if (PluginTags.Count < PluginSettings.Settings.GameLanguages.Count)
                {
                    foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages)
                    {
                        if (PluginTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                        {
                            PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                        }
                    }

                    foreach (Tag tag in PlayniteApi.Database.Tags)
                    {
                        if (tag.Name.IndexOf("[CL] ") > -1)
                        {
                            PluginTags.Add(tag);
                        }
                    }
                }
#if DEBUG
                logger.Debug($"{PluginName} [Ignored] - PluginTags: {JsonConvert.SerializeObject(PluginTags)}");
#endif
            }
            catch (Exception ex)
            {
                Common.LogError(ex, PluginName);
            }
        }

        public override void AddTag(Game game)
        {
            GameLocalizations gameLocalizations = Get(game, true);

            if (gameLocalizations.HasData)
            {
                try
                {
                    foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages.FindAll(x => x.IsTag && gameLocalizations.Items.Any(y => x.Name.ToLower() == y.Language.ToLower())))
                    {
                        Guid? TagId = FindGoodPluginTags($"[CL] { gameLanguage.DisplayName}");
                        if (TagId != null)
                        {
                            if (game.TagIds != null)
                            {
                                game.TagIds.Add((Guid)TagId);
                            }
                            else
                            {
                                game.TagIds = new List<Guid> { (Guid)TagId };
                            }
                        }
                    }

                    PlayniteApi.Database.Games.Update(game);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Common.LogError(ex, PluginName + " [Ignored]");
#endif
                    logger.Error($"{PluginName} - Tag insert error with {game.Name}");
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        $"{PluginName}-Tag-Errors",
                        $"{PluginName}\r\n" + resources.GetString("LOCCommonNotificationTagError"),
                        NotificationType.Error
                    ));
                }
            }
        }


        public override void SetThemesResources(Game game)
        {
            GameLocalizations gameLocalizations = Get(game, true);

            PluginSettings.Settings.HasData = gameLocalizations.HasData;
            PluginSettings.Settings.HasNativeSupport = gameLocalizations.HasNativeSupport();
            PluginSettings.Settings.ListNativeSupport = gameLocalizations.Items;
        }
    }
}
