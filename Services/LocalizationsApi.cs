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

        private readonly string _PluginUserDataPath;

        private PCGamingWikiLocalizations pCGamingWikiLocalizations;


        public LocalizationsApi(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
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

            GameLocalizations gameLocalizations = CheckLocalizations.PluginDatabase.GetDefault(game);
            gameLocalizations.Items = Localizations;

            return gameLocalizations;
        }
    }
}
