using CheckLocalizations.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using PluginCommon.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLocalizations.Services
{
    public class LocalizationsDatabase : PluginDatabaseObject<CheckLocalizationsSettings, GameLocalizationsCollection, GameLocalizations>
    {
        private LocalizationsApi localizationsApi;


        public LocalizationsDatabase(IPlayniteAPI PlayniteApi, CheckLocalizationsSettings PluginSettings, string PluginUserDataPath) : base(PlayniteApi, PluginSettings, PluginUserDataPath)
        {
            PluginName = "CheckLocalizations";

            ControlAndCreateDirectory(PluginUserDataPath, "CheckLocalizations");

            localizationsApi = new LocalizationsApi(PlayniteApi, PluginUserDataPath);
        }


        protected override bool LoadDatabase()
        {
            IsLoaded = false;
            Database = new GameLocalizationsCollection(PluginDatabaseDirectory);

            Database.SetGameInfo<Localization>(_PlayniteApi);

#if DEBUG
            logger.Debug($"{PluginName} - Database: {JsonConvert.SerializeObject(Database)}");
#endif

            GameSelectedData = new GameLocalizations();
            GetPluginTags();

            IsLoaded = true;
            return true;
        }


        public override GameLocalizations Get(Guid Id, bool OnlyCache = false)
        {
            GameIsLoaded = false;
            GameLocalizations gameLocalizations = GetOnlyCache(Id);
#if DEBUG
            logger.Debug($"{PluginName} - GetFromDb({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
            if (gameLocalizations == null && !OnlyCache)
            {
                ControlAndCreateDirectory(PluginUserDataPath, "CheckLocalizations");
                gameLocalizations = localizationsApi.GetLocalizations(Id);
                Add(gameLocalizations);

#if DEBUG
                logger.Debug($"{PluginName} - GetFromWeb({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
            }
            else if (gameLocalizations != null && !OnlyCache
                && gameLocalizations.Items.Where(x => x.IsManual == true).Count() != 0
                && gameLocalizations.Items.Where(x => x.IsManual == false).Count() == 0)
            {
                var dataWeb = localizationsApi.GetLocalizations(Id);
#if DEBUG
                logger.Debug($"{PluginName} - GetFromWebOnlyManual({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(dataWeb)}");
                logger.Debug($"{PluginName} - IsManualTrue({gameLocalizations.Items.Where(x => x.IsManual == true).Count()}) - IsManualFalse: {gameLocalizations.Items.Where(x => x.IsManual == false).Count()}");
#endif
                gameLocalizations.Items = gameLocalizations.Items.Concat(dataWeb.Items).ToList();

                Update(gameLocalizations);
            }
            else if (gameLocalizations == null)
            {
                Game game = _PlayniteApi.Database.Games.Get(Id);
                gameLocalizations = GetDefault(game);
                Add(gameLocalizations);
            }

            gameLocalizations.Items.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            GameIsLoaded = true;
            return gameLocalizations;
        }
        
        public bool RemoveWithManual(Guid Id)
        {
            try
            {
                GameLocalizations gameLocalizations = GetOnlyCache(Id);

                if (gameLocalizations.Items.Where(x => x.IsManual).Count() == 0)
                {
                    return Remove(Id);
                }
                else
                {
                    var ItemsManual = gameLocalizations.Items.Where(x => x.IsManual).ToList();
                    gameLocalizations.Items = null;
                    gameLocalizations.Items = ItemsManual;
#if DEBUG
                    logger.Debug($"{PluginName} - RemoveWithoutManual({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
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
            logger.Debug($"{PluginName} - GetPluginTags()");
#endif

            try
            {
                // Get tags in playnite database
                PluginTags = new List<Tag>();
                foreach (Tag tag in _PlayniteApi.Database.Tags)
                {
                    if (tag.Name.IndexOf("[CL] ") > -1)
                    {
                        PluginTags.Add(tag);
                    }
                }

                // Add missing tags
                if (PluginTags.Count < PluginSettings.GameLanguages.Count)
                {
                    foreach (GameLanguage gameLanguage in PluginSettings.GameLanguages)
                    {
                        if (PluginTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                        {
                            _PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                        }
                    }

                    foreach (Tag tag in _PlayniteApi.Database.Tags)
                    {
                        if (tag.Name.IndexOf("[CL] ") > -1)
                        {
                            PluginTags.Add(tag);
                        }
                    }
                }

#if DEBUG
                logger.Debug($"{PluginName} - PluginTags: {JsonConvert.SerializeObject(PluginTags)}");
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
                    foreach (GameLanguage gameLanguage in PluginSettings.GameLanguages.FindAll(x => x.IsTag && gameLocalizations.Items.Any(y => x.Name.ToLower() == y.Language.ToLower())))
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

                    _PlayniteApi.Database.Games.Update(game);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Common.LogError(ex, PluginName);
#endif
                    logger.Error($"{PluginName} - Tag insert error with {game.Name}");
                    _PlayniteApi.Notifications.Add(new NotificationMessage(
                        $"{PluginName}-Tag-Errors",
                        $"{PluginName}\r\n" + resources.GetString("LOCCommonNotificationTagError"),
                        NotificationType.Error
                    ));
                }
            }
        }
    }
}
