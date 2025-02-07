using CheckLocalizations.Clients;
using CheckLocalizations.Models;
using CommonPluginsShared;
using CommonPluginsShared.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLocalizations.Services
{
    public class LocalizationsApi : IDisposable
    {
        private static ILogger Logger => LogManager.GetLogger();

        private PCGamingWikiLocalizations PCGamingWikiLocalizations { get; set; }
        private SteamLocalizations SteamLocalizations { get; set; }


        public LocalizationsApi()
        {
            PCGamingWikiLocalizations = new PCGamingWikiLocalizations();
            SteamLocalizations = new SteamLocalizations();
        }


        public GameLocalizations GetLocalizations(Guid Id)
        {
            return GetLocalizations(API.Instance.Database.Games.Get(Id));
        }

        public GameLocalizations GetLocalizations(Game game)
        {
            Thread.Sleep(1000);

            GameLocalizations gameLocalizations = CheckLocalizations.PluginDatabase.GetDefault(game);

            List<Localization> LocalizationsGamingWiki = new List<Localization>();
            List<Localization> LocalizationsSteam = new List<Localization>();

            Task[] tasks = new Task[2];
            tasks[0] = Task.Run(() => { LocalizationsGamingWiki = PCGamingWikiLocalizations.GetLocalizations(game); });
            tasks[1] = Task.Run(() => { LocalizationsSteam = SteamLocalizations.GetLocalizations(game); });

            Task.WaitAll(tasks);

            List<Localization> Localizations = new List<Localization>();
            if (LocalizationsGamingWiki.Count >= LocalizationsSteam.Count)
            {
                Localizations = LocalizationsGamingWiki;
                Common.LogDebug(true, $"Used PCGamingWikiLocalizations for {game.Name} - {Serialization.ToJson(Localizations)}");

                gameLocalizations.SourcesLink = new SourceLink { Name = "PCGamingWiki", GameName = PCGamingWikiLocalizations.GetGameName(), Url = PCGamingWikiLocalizations.GetUrl() };
            }
            else
            {
                // Merged PCGamingWikiLocalizations with SteamLocalizations
                Localizations = LocalizationsSteam.Select(x => new Localization
                {
                    Language = x.Language,
                    Audio = LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)) != null ? LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)).Audio : x.Audio,
                    Notes = LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)) != null ? LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)).Notes : x.Notes,
                    Sub = LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)) != null ? LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)).Sub : x.Sub,
                    Ui = LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)) != null ? LocalizationsGamingWiki.Find(y => y.Language.IsListEqual(x.Language)).Ui : x.Ui,
                    IsManual = x.IsManual
                }).ToList();
                Common.LogDebug(true, $"Used Steam for {game.Name} - {Serialization.ToJson(Localizations)}");

                gameLocalizations.SourcesLink = new SourceLink { Name = "Steam", GameName = SteamLocalizations.GetGameName(), Url = SteamLocalizations.GetUrl() };
            }

            gameLocalizations.Items = Localizations;

            return gameLocalizations;
        }


        public void Dispose()
        {
            PCGamingWikiLocalizations = null;
            SteamLocalizations = null;
        }
    }
}
