using CheckLocalizations.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLocalizations.Services
{
    public class OldToNew
    {
        private ILogger logger = LogManager.GetLogger();

        public bool IsOld = false;

        private string PathActivityDB = "CheckLocalizations";
        private string PathActivityManualDB = "CheckLocalizationsManual";

        public ConcurrentDictionary<Guid, List<GameLocalizationOld>> Items { get; set; } = new ConcurrentDictionary<Guid, List<GameLocalizationOld>>();
        public ConcurrentDictionary<Guid, List<GameLocalizationOld>> ItemsManual { get; set; } = new ConcurrentDictionary<Guid, List<GameLocalizationOld>>();


        public OldToNew(string PluginUserDataPath)
        {
            PathActivityDB = Path.Combine(PluginUserDataPath, PathActivityDB);
            PathActivityManualDB = Path.Combine(PluginUserDataPath, PathActivityManualDB);

            if (Directory.Exists(PathActivityDB) && Directory.Exists(PathActivityManualDB))
            {
                Directory.Move(PathActivityDB, PathActivityDB + "_old");
                Directory.Move(PathActivityManualDB, PathActivityManualDB + "_old");

                PathActivityDB += "_old";
                PathActivityManualDB += "_old";

                LoadOldDB();
                IsOld = true;
            }
        }

        public void LoadOldDB()
        {
            logger.Info($"CheckLocalizations - LoadOldDB()");

            Parallel.ForEach(Directory.EnumerateFiles(PathActivityDB, "*.json"), (objectFile) =>
            {
                string objectFileManual = string.Empty;

                try
                {
                    var JsonStringData = File.ReadAllText(objectFile);

#if DEBUG
                    logger.Debug(objectFile.Replace(PathActivityDB, "").Replace(".json", "").Replace("\\", ""));
#endif
                    Guid gameId = Guid.Parse(objectFile.Replace(PathActivityDB, "").Replace(".json", "").Replace("\\", ""));


                    List<GameLocalizationOld> CheckLocalizations = JsonConvert.DeserializeObject<List<GameLocalizationOld>>(JsonStringData);
                    List<GameLocalizationOld> CheckLocalizationsManual = new List<GameLocalizationOld>();

                    objectFileManual = PathActivityManualDB + "\\" + objectFile.Replace(PathActivityDB, "");
                    if (File.Exists(objectFileManual))
                    {
                        var JsonStringDataManual = File.ReadAllText(objectFileManual);
                        CheckLocalizationsManual = JsonConvert.DeserializeObject<List<GameLocalizationOld>>(JsonStringDataManual);

                        ItemsManual.TryAdd(gameId, CheckLocalizationsManual);
                    };

                    Items.TryAdd(gameId, CheckLocalizations);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", $"Failed to load item from {objectFile} or {objectFileManual}");
                }
            });

            logger.Info($"CheckLocalizations - Find {Items.Count} & {ItemsManual.Count} items");
        }

        public void ConvertDB(IPlayniteAPI PlayniteApi)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                "CheckLocalizations - Database migration",
                false
            );
            globalProgressOptions.IsIndeterminate = true;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                logger.Info($"CheckLocalizations - ConvertDB()");

                int Converted = 0;

                foreach (var item in Items)
                {
                    try
                    {
                        if (PlayniteApi.Database.Games.Get(item.Key) != null)
                        {
                            GameLocalizations gameLocalizations = CheckLocalizations.PluginDatabase.Get(item.Key, true);

                            foreach (var localization in item.Value)
                            {
                                gameLocalizations.Items.Add(new Localization
                                {
                                    Language = localization.Language,
                                    Audio = localization.Audio,
                                    Ui = localization.Ui,
                                    Sub = localization.Sub,
                                    IsManual = false,
                                    Notes = localization.Notes
                                });
                            }

                            ItemsManual.TryGetValue(item.Key, out List<GameLocalizationOld> localizationManual);

                            if (localizationManual != null && localizationManual.Count > 0)
                            {
                                foreach (var localization in localizationManual)
                                {
                                    gameLocalizations.Items.Add(new Localization
                                    {
                                        Language = localization.Language,
                                        Audio = localization.Audio,
                                        Ui = localization.Ui,
                                        Sub = localization.Sub,
                                        IsManual = true,
                                        Notes = localization.Notes
                                    });
                                }
                            }

                            Thread.Sleep(10);
                            CheckLocalizations.PluginDatabase.Add(gameLocalizations);
                            Converted++;
                        }
                        else
                        {
                            logger.Warn($"CheckLocalizations - Game is deleted - {item.Key.ToString()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, "CheckLocalizations", $"Failed to load ConvertDB from {item.Key.ToString()}");
                    }
                }

                logger.Info($"CheckLocalizations - Converted {Converted} / {Items.Count}");

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"CheckLocalizations - Migration - {String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }, globalProgressOptions);

            IsOld = false;
        }
    }


    public class GameLocalizationOld
    {
        public string Language { get; set; }
        public SupportStatus Ui { get; set; }
        public SupportStatus Audio { get; set; }
        public SupportStatus Sub { get; set; }
        public string Notes { get; set; }
    }
}
