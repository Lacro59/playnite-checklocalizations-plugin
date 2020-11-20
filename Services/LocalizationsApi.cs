using CheckLocalizations.Clients;
using CheckLocalizations.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Services
{
    public class LocalizationsApi
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();
        private IPlayniteAPI _PlayniteApi;

        private CheckLocalizationsSettings _settings;
        private readonly string _PluginUserDataPath;

        private PCGamingWikiLocalizations pCGamingWikiLocalizations;


        public LocalizationsApi(IPlayniteAPI PlayniteApi, CheckLocalizationsSettings settings, string PluginUserDataPath)
        {
            _settings = settings;
            _PlayniteApi = PlayniteApi;
            _PluginUserDataPath = PluginUserDataPath;

            pCGamingWikiLocalizations = new PCGamingWikiLocalizations(_PlayniteApi, _PluginUserDataPath);
        }


        public GameLocalizations GetLocalizations(Guid Id)
        {
            return GetLocalizations(_PlayniteApi.Database.Games.Get(Id));
        }

        public GameLocalizations GetLocalizations(Game game)
        {
            List<Localization> Localizations = pCGamingWikiLocalizations.GetLocalizations(game);

            GameLocalizations gameLocalizations = new GameLocalizations
            {
                Id = game.Id,
                Name = game.Name,
                Hidden = game.Hidden,
                Icon = game.Icon,
                CoverImage = game.CoverImage,
                Items = Localizations
            };

            AddTag(game, _settings.EnableTag, Localizations);

            return gameLocalizations;
        }


        public void RemoveTag(Game game)
        {
            // Tags id
            List<Tag> ClTags = new List<Tag>();
            foreach (Tag tag in _PlayniteApi.Database.Tags)
            {
                if (tag.Name.IndexOf("[CL]") > -1)
                {
                    ClTags.Add(tag);
                }
            }
            // Add missing tags in database
            if (ClTags.Count < _settings.GameLanguages.Count)
            {
                foreach (GameLanguage gameLanguage in _settings.GameLanguages)
                {
                    if (ClTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                    {
                        _PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                    }
                }

                foreach (Tag tag in _PlayniteApi.Database.Tags)
                {
                    if (tag.Name.IndexOf("[CL]") > -1)
                    {
                        ClTags.Add(tag);
                    }
                }
            }

            // Remove tag
            if (game.Tags != null && game.Tags.Count > 0)
            {
                foreach (Tag tag in ClTags)
                {
                    game.TagIds.Remove(tag.Id);
                }
            }
        }

        private void AddTag(Game game, bool EnableTag, List<Models.Localization> gameLocalizations)
        {
            // Tags id
            List<Tag> ClTags = new List<Tag>();
            foreach (Tag tag in _PlayniteApi.Database.Tags)
            {
                if (tag.Name.IndexOf("[CL]") > -1)
                {
                    ClTags.Add(tag);
                }
            }
            // Add missing tags in database
            if (ClTags.Count < _settings.GameLanguages.Count)
            {
                foreach(GameLanguage gameLanguage in _settings.GameLanguages)
                {
                    if (ClTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                    {
                        _PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                    }
                }

                foreach (Tag tag in _PlayniteApi.Database.Tags)
                {
                    if (tag.Name.IndexOf("[CL]") > -1)
                    {
                        ClTags.Add(tag);
                    }
                }
            }

            // Add or remove tag
            if (gameLocalizations != null && gameLocalizations.Count > 0)
            {
                // Remove tag
                if (game.Tags != null && game.Tags.Count > 0)
                {
                    foreach (Tag tag in ClTags)
                    {
                        game.TagIds.Remove(tag.Id);
                    }
                }

                List<Guid> tagIds = new List<Guid>();
                if (game.TagIds != null)
                {
                    tagIds = game.TagIds;
                }

                if (EnableTag)
                {
                    try
                    {
                        foreach (GameLanguage gameLanguage in _settings.GameLanguages)
                        {
                            if (gameLanguage.IsTag)
                            {
                                if (gameLocalizations.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) != null)
                                {
                                    logger.Info($"CheckLocalizations - Add tag [CL] {gameLanguage.DisplayName} for {game.Name}");
                                    tagIds.Add((ClTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}")).Id);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, "CheckLocalizations", $"Tag insert error in {game.Name}");
                    }
                }

                if (tagIds.Count > 0)
                {
                    game.TagIds = tagIds;
                }
                _PlayniteApi.Database.Games.Update(game);
            }
        }


        public void AddAllTagFromMain(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                resources.GetString("LOCCommonAddingAllTag"),
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                var db = PlayniteApi.Database.Games.Where(x => x.Hidden == false);
                activateGlobalProgress.ProgressMaxValue = (double)db.Count();

                string CancelText = string.Empty;

                foreach (Game game in db)
                {
                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                    {
                        CancelText = " canceled";
                        break;
                    }

                    AddTag(game, _settings.EnableTag, CheckLocalizations.PluginDatabase.Get(game, true).Items);
                    activateGlobalProgress.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"CheckLocalizations - Task AddAllTagFromMain(){CancelText} - {String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }, globalProgressOptions);
        }

        public void RemoveAllTagFromMain(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                resources.GetString("LOCCommonRemovingAllTag"),
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                var db = PlayniteApi.Database.Games.Where(x => x.Hidden == false);
                activateGlobalProgress.ProgressMaxValue = (double)db.Count();

                string CancelText = string.Empty;

                foreach (Game game in db)
                {
                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                    {
                        CancelText = " canceled";
                        break;
                    }

                    RemoveTag(game);
                    activateGlobalProgress.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"CheckLocalizations - Task RemoveAllTagFromMain(){CancelText} - {String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }, globalProgressOptions);
        }
    }
}
