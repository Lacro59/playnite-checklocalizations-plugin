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
        private IPlayniteAPI _PlayniteApi;

        private CheckLocalizationsSettings _settings;

        private string _PluginUserDataPath { get; set; }

        private string PluginDirectory { get; set; }
        private string PluginDirectoryManual { get; set; }


        public LocalizationsApi(string PluginUserDataPath, IPlayniteAPI PlayniteApi, CheckLocalizationsSettings settings)
        {
            _settings = settings;
            _PlayniteApi = PlayniteApi;
            _PluginUserDataPath = PluginUserDataPath;

            PluginDirectory = Path.Combine(_PluginUserDataPath, "CheckLocalizations");
            if (!Directory.Exists(PluginDirectory))
            {
                Directory.CreateDirectory(PluginDirectory);
            }

            PluginDirectoryManual = Path.Combine(_PluginUserDataPath, "CheckLocalizationsManual");
            if (!Directory.Exists(PluginDirectoryManual))
            {
                Directory.CreateDirectory(PluginDirectoryManual);
            }
        }


        public void RemoveLocalizations(Game game)
        {
            string FileGameData = Path.Combine(PluginDirectory, game.Id.ToString() + ".json");

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

        public void RemoveLocalizationsManual(Game game)
        {
            string FileGameData = Path.Combine(PluginDirectoryManual, game.Id.ToString() + ".json");

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
                    _PlayniteApi.Dialogs.ShowMessage(resources.GetString("LOCCommonDataRemove"), "CheckLocalizations");
                }
                catch
                {
                    _PlayniteApi.Dialogs.ShowErrorMessage(resources.GetString("LOCCommonDataErrorRemove"), "CheckLocalizations");
                }
            }
        }


        public List<GameLocalization> GetLocalizations(Game game, bool CacheOnly = false, bool WithoutManual = false)
        {
            List<GameLocalization> gameLocalizations = new List<GameLocalization>();

            if (!Directory.Exists(PluginDirectory))
            {
                Directory.CreateDirectory(PluginDirectory);
            }

            // Get cache
            string FileGameLocalizations = Path.Combine(PluginDirectory, game.Id.ToString() + ".json");
            if (File.Exists(FileGameLocalizations))
            {
#if DEBUG
                logger.Debug($"CheckLocalizations - Find from cache for {game.Name}");
#endif
                gameLocalizations = JsonConvert.DeserializeObject<List<GameLocalization>>(File.ReadAllText(FileGameLocalizations));
            }

            // Get datas
            if (!CacheOnly)
            {
                PCGamingWikiLocalizations pCGamingWikiLocalizations = new PCGamingWikiLocalizations(game, _PluginUserDataPath, _PlayniteApi);
                gameLocalizations = pCGamingWikiLocalizations.GetLocalizations();

                // Save datas
                File.WriteAllText(FileGameLocalizations, JsonConvert.SerializeObject(gameLocalizations));
            }

            // Get manual
            if (!WithoutManual)
            {
                gameLocalizations = gameLocalizations.Concat(GetLocalizationsManual(game)).ToList();
            }

            AddTag(game, _settings.EnableTag, gameLocalizations);

            return gameLocalizations;
        }

        public List<GameLocalization> GetLocalizationsManual(Game game)
        {
            List<GameLocalization> gameLocalizations = new List<GameLocalization>();

            if (!Directory.Exists(PluginDirectoryManual))
            {
                Directory.CreateDirectory(PluginDirectoryManual);
            }

            // Get cache
            string FileGameLocalizations = Path.Combine(PluginDirectoryManual, game.Id.ToString() + ".json");
            if (File.Exists(FileGameLocalizations))
            {
                return JsonConvert.DeserializeObject<List<GameLocalization>>(File.ReadAllText(FileGameLocalizations));
            }

            return gameLocalizations;
        }


        public void SaveLocalizationsManual(List<GameLocalization> gameLocalizationsManual, Game game)
        {
            // Save datas
            string FileGameLocalizations = Path.Combine(PluginDirectoryManual, game.Id.ToString() + ".json");
            try
            {
                File.WriteAllText(FileGameLocalizations, JsonConvert.SerializeObject(gameLocalizationsManual));
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", $"Error on SaveLocalizationsManual()");
            }
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

        private void AddTag(Game game, bool EnableTag, List<GameLocalization> gameLocalizations)
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

                    AddTag(game, _settings.EnableTag, GetLocalizations(game, true));
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

        public void GetAllDataFromMain(IPlayniteAPI PlayniteApi, string PluginUserDataPath, CheckLocalizationsSettings settings)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                resources.GetString("LOCCommonGettingAllDatas"),
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
