using CheckLocalizations.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.API;
using PluginCommon.PlayniteResources.Common;
using PluginCommon.PlayniteResources.Converters;
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
        private IPlayniteAPI PlayniteApi;

        private CheckLocalizationsSettings settings;

        private string PluginUserDataPath { get; set; }
        private string PluginDirectory { get; set; }


        public LocalizationsApi(string PluginUserDataPath, IPlayniteAPI PlayniteApi, CheckLocalizationsSettings settings)
        {
            this.settings = settings;
            this.PlayniteApi = PlayniteApi;
            this.PluginUserDataPath = PluginUserDataPath;
            PluginDirectory = PluginUserDataPath + "\\CheckLocalizations\\";
        }

        public void RemoveLocalizations(Game game)
        {
            string FileGameData = PluginUserDataPath + "\\CheckLocalizations\\" + game.Id.ToString() + ".json";

            if (File.Exists(FileGameData))
            {
                File.Delete(FileGameData);

                var TaskIntegrationUI = Task.Run(() =>
                {
                    CheckLocalizations.checkLocalizationsUI.RefreshElements(game);
                });
            }
            else
            {
                logger.Warn($"CheckLocalizations - Impossible to remove {game.Name} in {FileGameData}");
            }
        }

        public void ClearAllData()
        {
            if (Directory.Exists(PluginDirectory))
            {
                try
                {
                    Directory.Delete(PluginDirectory, true);
                    Directory.CreateDirectory(PluginDirectory);
                    PlayniteApi.Dialogs.ShowMessage(resources.GetString("LOCCheckLocalizationsRemove"), "CheckLocalizations");
                }
                catch
                {
                    PlayniteApi.Dialogs.ShowErrorMessage(resources.GetString("LOCCheckLocalizationsErrorRemove"), "CheckLocalizations");
                }
            }
        }


        public List<GameLocalization> GetLocalizations(Game game, bool CacheOnly = false)
        {
            List<GameLocalization> gameLocalizations = new List<GameLocalization>();

            if (!Directory.Exists(PluginDirectory))
            {
                Directory.CreateDirectory(PluginDirectory);
            }

            // Get cache
            string FileGameLocalizationss = PluginDirectory + "\\" + game.Id.ToString() + ".json";
            if (File.Exists(FileGameLocalizationss))
            {
                logger.Info($"CheckLocalizations - Find from cache for {game.Name}");
                return JsonConvert.DeserializeObject<List<GameLocalization>>(File.ReadAllText(FileGameLocalizationss));
            }

            // Get datas
            if (!CacheOnly)
            {
                PCGamingWikiLocalizations pCGamingWikiLocalizations = new PCGamingWikiLocalizations(game, PluginUserDataPath, PlayniteApi);
                gameLocalizations = pCGamingWikiLocalizations.GetLocalizations();

                // Save datas
                File.WriteAllText(FileGameLocalizationss, JsonConvert.SerializeObject(gameLocalizations));
            }

            AddTag(game, settings.EnableTag, gameLocalizations);


            return gameLocalizations;
        }


        public void RemoveTag(Game game)
        {
            // Tags id
            List<Tag> ClTags = new List<Tag>();
            foreach (Tag tag in PlayniteApi.Database.Tags)
            {
                if (tag.Name.IndexOf("[CL]") > -1)
                {
                    ClTags.Add(tag);
                }
            }
            // Add missing tags in database
            if (ClTags.Count < settings.GameLanguages.Count)
            {
                foreach (GameLanguage gameLanguage in settings.GameLanguages)
                {
                    if (ClTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                    {
                        PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                    }
                }

                foreach (Tag tag in PlayniteApi.Database.Tags)
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

        private void AddTag(Game game, bool EnableTag, List<GameLocalization> gameLocalizations)
        {
            // Tags id
            List<Tag> ClTags = new List<Tag>();
            foreach (Tag tag in PlayniteApi.Database.Tags)
            {
                if (tag.Name.IndexOf("[CL]") > -1)
                {
                    ClTags.Add(tag);
                }
            }
            // Add missing tags in database
            if (ClTags.Count < settings.GameLanguages.Count)
            {
                foreach(GameLanguage gameLanguage in settings.GameLanguages)
                {
                    if (ClTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                    {
                        PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                    }
                }

                foreach (Tag tag in PlayniteApi.Database.Tags)
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
                        foreach (GameLanguage gameLanguage in settings.GameLanguages)
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
                PlayniteApi.Database.Games.Update(game);
            }
        }


        public void AddAllTagFromMain(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                resources.GetString("LOCCheckLocalizationsAddingAllTag"),
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

                    AddTag(game, settings.EnableTag, GetLocalizations(game, true));
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
                resources.GetString("LOCCheckLocalizationsRemovingAllTag"),
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

        public void GetAllDataFromMain(IPlayniteAPI PlayniteApi, string PluginUserDataPath, CheckLocalizationsSettings settings)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                resources.GetString("LOCCheckLocalizationsGettingAllDatas"),
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

                    GetLocalizations(game);
                    activateGlobalProgress.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"CheckLocalizations - Task GetAllDataFromMain(){CancelText} - {String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }, globalProgressOptions);
        }
    }
}
