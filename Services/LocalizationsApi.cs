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
            List<Localization> Localizations = pCGamingWikiLocalizations.GetLocalizations(game);
            if (Localizations.Count == 0)
            {
                Localizations = steamLocalizations.GetLocalizations(game);

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
