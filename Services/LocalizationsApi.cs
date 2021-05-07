using CheckLocalizations.Clients;
using CheckLocalizations.Models;
using CommonPluginsShared;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLocalizations.Services
{
    public class LocalizationsApi : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();
        private IPlayniteAPI _PlayniteApi;

        private readonly string _PluginUserDataPath;

        private PCGamingWikiLocalizations pCGamingWikiLocalizations;
        private SteamLocalizations steamLocalizations;


        public LocalizationsApi(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
            _PlayniteApi = PlayniteApi;
            _PluginUserDataPath = PluginUserDataPath;

            pCGamingWikiLocalizations = new PCGamingWikiLocalizations(_PlayniteApi, _PluginUserDataPath);
            steamLocalizations = new SteamLocalizations(_PlayniteApi, _PluginUserDataPath);
        }


        public GameLocalizations GetLocalizations(Guid Id)
        {
            return GetLocalizations(_PlayniteApi.Database.Games.Get(Id));
        }

        public GameLocalizations GetLocalizations(Game game)
        {
            Thread.Sleep(1000);

            List<Localization> LocalizationsGamingWiki = new List<Localization>();
            List<Localization> LocalizationsSteam = new List<Localization>();

            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => { LocalizationsGamingWiki = pCGamingWikiLocalizations.GetLocalizations(game); });
            tasks[1] = Task.Run(() => { LocalizationsSteam = steamLocalizations.GetLocalizations(game); });

            Task.WaitAll(tasks);

            List<Localization> Localizations = new List<Localization>();
            if (LocalizationsGamingWiki.Count >= LocalizationsSteam.Count)
            {
                Localizations = LocalizationsGamingWiki;
                Common.LogDebug(true, $"Used PCGamingWikiLocalizations for {game.Name} - {JsonConvert.SerializeObject(Localizations)}");
            }
            else
            {
                Localizations = LocalizationsSteam;
                Common.LogDebug(true, $"Used Steam for {game.Name} - {JsonConvert.SerializeObject(Localizations)}");
            }

            GameLocalizations gameLocalizations = CheckLocalizations.PluginDatabase.GetDefault(game);
            gameLocalizations.Items = Localizations;

            return gameLocalizations;
        }


        public void Dispose()
        {
            pCGamingWikiLocalizations = null;
            steamLocalizations = null;
        }
    }
}
