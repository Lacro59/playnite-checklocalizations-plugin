using CheckLocalizations.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon.Collections;
using PluginCommon.PlayniteResources.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Services
{
    public class LocalizationsDatabase : PluginDatabaseObject
    {
        private GameLocalizationsCollection db;
        private LocalizationsApi localizationsApi;


        public LocalizationsDatabase(IPlayniteAPI PlayniteApi, CheckLocalizationsSettings PluginSettings, string PluginUserDataPath) : base(PlayniteApi, PluginSettings, PluginUserDataPath)
        {
            PluginName = "CheckLocalizations";

            ControlAndCreateDirectory(PluginUserDataPath, "CheckLocalizations");

            localizationsApi = new LocalizationsApi(PlayniteApi, PluginSettings, PluginUserDataPath);
        }

        protected override bool LoadDatabase()
        {
            db = new GameLocalizationsCollection(PluginDatabaseDirectory);

            db.SetGameInfo<Localization>(_PlayniteApi);
#if DEBUG
            logger.Debug($"{PluginName} - db: {JsonConvert.SerializeObject(db)}");
#endif

            IsLoaded = true;
            return true;
        }


        public GameLocalizations Get(Guid Id)
        {
            GameLocalizations gameLocalizations = db.Get(Id);
#if DEBUG
            logger.Debug($"CheckLocalizations - Get({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
            if (gameLocalizations == null)
            {
                ControlAndCreateDirectory(PluginUserDataPath, "CheckLocalizations");
                gameLocalizations = localizationsApi.GetLocalizations(Id);
#if DEBUG
                logger.Debug($"CheckLocalizations - Get({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
                Add(gameLocalizations);
            }

            gameLocalizations.Data.Sort((x, y) => x.Language.CompareTo(y.Language));

            return gameLocalizations;
        }
        
        public GameLocalizations Get(Game game)
        {
            return Get(game.Id);
        }

        public void Add(GameLocalizations itemToAdd)
        {
            db.Add(itemToAdd);
        }

        public void Update(GameLocalizations itemToUpdate)
        {
            db.Update(itemToUpdate);
        }

        public bool Remove(Guid Id, bool WithManual = false)
        {
            GameLocalizations gameLocalizations = Get(Id);

            if (WithManual && gameLocalizations.Data.Where(x => x.IsManual = true).Count() == 0)
            {
                return db.Remove(Id);
            }
            
            gameLocalizations.Data = gameLocalizations.Data.Where(x => x.IsManual = true).ToList();
            Update(gameLocalizations);

            return true;
        }


        public void GetAllDataFromMain()
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                resources.GetString("LOCCommonGettingAllDatas"),
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            _PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                var PlayniteDb = _PlayniteApi.Database.Games.Where(x => x.Hidden == false);
                activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                string CancelText = string.Empty;

                foreach (Game game in PlayniteDb)
                {
                    if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                    {
                        CancelText = " canceled";
                        break;
                    }

                    Get(game);
                    activateGlobalProgress.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                logger.Info($"CheckLocalizations - Task GetAllDataFromMain(){CancelText} - {String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }, globalProgressOptions);
        }


        public void RemoveTag(Game game)
        {
            localizationsApi.RemoveTag(game);
        }


        public void AddAllTagFromMain()
        {
            localizationsApi.AddAllTagFromMain(_PlayniteApi, PluginUserDataPath);
        }

        public void RemoveAllTagFromMain()
        {
            localizationsApi.RemoveAllTagFromMain(_PlayniteApi, PluginUserDataPath);
        }
    }
}
