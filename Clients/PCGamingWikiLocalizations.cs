using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CheckLocalizations.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLocalizations.Clients
{
    public class PCGamingWikiLocalizations
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI PlayniteApi;

        private readonly string urlSteamId = "https://pcgamingwiki.com/api/appid.php?appid={0}";
        private string urlPCGamingWiki = string.Empty;
        private readonly int SteamId = 0;

        private Game game;


        public PCGamingWikiLocalizations(Game game, string PluginUserDataPath, IPlayniteAPI PlayniteApi)
        {
            this.PlayniteApi = PlayniteApi;
            this.game = game;

            if (game.SourceId != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                if (game.Source.Name.ToLower() == "steam")
                {
                    SteamId = int.Parse(game.GameId);
                }
            }
            if (SteamId == 0)
            {
                SteamApi steamApi = new SteamApi(PluginUserDataPath);
                SteamId = steamApi.GetSteamId(game.Name);
            }

            if (game.Links != null)
            {
                foreach (Link link in game.Links)
                {
                    if (link.Url.ToLower().Contains("pcgamingwiki"))
                    {
                        urlPCGamingWiki = link.Url;
                    }
                }
            }

#if DEBUG
            logger.Debug($"CheckLocalizations - PCGamingWikiLocalizations - {game.Name} - SteamId: {SteamId} - urlPCGamingWiki: {urlPCGamingWiki}");
#endif
        }

        public List<Localization> GetLocalizations()
        {
            List<Localization> gameLocalizations = new List<Localization>();

            // Search data with SteamId (is find) or game url (if defined)
            if (SteamId != 0)
            {
                gameLocalizations = GetLocalizations(string.Format(urlSteamId, SteamId));
                if (gameLocalizations.Count > 0)
                {
                    return gameLocalizations;
                }
            }
            if (!urlPCGamingWiki.IsNullOrEmpty())
            {
                gameLocalizations = GetLocalizations(urlPCGamingWiki);
                if (gameLocalizations.Count > 0)
                {
                    return gameLocalizations;
                }
            }

            logger.Warn($"CheckLocalizations - PCGamingWikiLocalizations - Not find for {game.Name}");

            return gameLocalizations;
        }

        public List<Localization> GetLocalizations(string url)
        {
            List<Localization> gameLocalizations = new List<Localization>();

            try
            {
                logger.Debug($"CheckLocalizations - url {url}");

                // Get data & parse
                string ResultWeb = Web.DownloadStringData(url).GetAwaiter().GetResult();
                HtmlParser parser = new HtmlParser();
                IHtmlDocument HtmlLocalization = parser.Parse(ResultWeb);

                foreach (var row in HtmlLocalization.QuerySelectorAll("tr.table-l10n-body-row"))
                {
                    string Language = Regex.Replace(row.QuerySelector("th").InnerHtml, "<.+?>(.*)<.+?>", "$1");
                    SupportStatus Ui = SupportStatus.Unknown;
                    SupportStatus Audio = SupportStatus.Unknown;
                    SupportStatus Sub = SupportStatus.Unknown;
                    string Notes = string.Empty;

                    int i = 1;
                    foreach (var td in row.QuerySelectorAll("td"))
                    {
                        switch (i)
                        {
                            case 1:
                                Ui = GetSupportStatus(td.QuerySelector("div").GetAttribute("title"));
                                break;
                            case 2:
                                Audio = GetSupportStatus(td.QuerySelector("div").GetAttribute("title"));
                                break;
                            case 3:
                                Sub = GetSupportStatus(td.QuerySelector("div").GetAttribute("title"));
                                break;
                            case 4:
                                Notes = Regex.Replace(td.InnerHtml, "<.+?>(.*)<.+?>", "$1");
                                break;
                        }
                        i++;
                    }

                    gameLocalizations.Add(new Models.Localization
                    {
                        Language = Language,
                        Ui = Ui,
                        Audio = Audio,
                        Sub = Sub,
                        Notes = Notes
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", "Error on PCGamingWikiLocalizations.GetLocalizations()");
            }

            return gameLocalizations;
        }

        private SupportStatus GetSupportStatus(string title)
        {
            switch (title.ToLower())
            {
                case "native support":
                    return SupportStatus.Native;
                case "no native support":
                    return SupportStatus.NoNative;
                case "hackable":
                    return SupportStatus.Hackable;
                case "not applicable":
                    return SupportStatus.NotApplicable;
                default:
                    return SupportStatus.Unknown;
            }
        }
    }
}
