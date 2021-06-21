using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CheckLocalizations.Models;
using CommonPluginsShared;
using CommonPluginsStores;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLocalizations.Clients
{
    public class PCGamingWikiLocalizations
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _PlayniteApi;

        private readonly string _PluginUserDataPath;

        private SteamApi steamApi;
        private readonly string urlSteamId = "https://pcgamingwiki.com/api/appid.php?appid={0}";
        private string UrlPCGamingWikiSearch { get; set; } = @"https://pcgamingwiki.com/w/index.php?search=";
        private string UrlPCGamingWiki { get; set; } = @"https://www.pcgamingwiki.com";


        public PCGamingWikiLocalizations(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
            _PlayniteApi = PlayniteApi;
            _PluginUserDataPath = PluginUserDataPath;

            steamApi = new SteamApi();
        }


        private string GetUrlIsOneResult(string WebResponse)
        {
            string url = string.Empty;

            try
            {
                if (!WebResponse.Contains("There were no results matching the query"))
                {
                    HtmlParser parser = new HtmlParser();
                    IHtmlDocument HtmlDocument = parser.Parse(WebResponse);

                    if (HtmlDocument.QuerySelectorAll("ul.mw-search-results")?.Count() == 2)
                    {
                        var TitleMatches = HtmlDocument.QuerySelectorAll("ul.mw-search-results")[0].QuerySelectorAll("li");
                        if (TitleMatches?.Count() == 1)
                        {
                            url = UrlPCGamingWiki + TitleMatches[0].QuerySelector("a").GetAttribute("href");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return url;
        }


        private string FindGoodUrl(Game game, int SteamId = 0)
        {
            string url = string.Empty;
            string urlMatch = string.Empty;
            string WebResponse = string.Empty;


            url = string.Empty;
            if (SteamId != 0)
            {
                url = string.Format(urlSteamId, SteamId);

                Thread.Sleep(1000);
                WebResponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
                if (!WebResponse.ToLower().Contains("search results"))
                {
                    return url;
                }
            }


            url = string.Empty;
            if (game.Links != null)
            {
                foreach (Link link in game.Links)
                {
                    if (link.Url.ToLower().Contains("pcgamingwiki"))
                    {
                        url = link.Url;

                        if (url.Contains(@"http://pcgamingwiki.com/w/index.php?search="))
                        {
                            url = UrlPCGamingWikiSearch + WebUtility.UrlEncode(url.Replace(@"http://pcgamingwiki.com/w/index.php?search=", string.Empty));
                        }
                        if (url.Length == UrlPCGamingWikiSearch.Length)
                        {
                            url =  string.Empty;
                        }
                    }
                }

                if (!url.IsNullOrEmpty())
                {
                    Thread.Sleep(1000);
                    WebResponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
                    if (!WebResponse.ToLower().Contains("search results"))
                    {
                        return url;
                    }
                    else
                    {
                        urlMatch = GetUrlIsOneResult(WebResponse);
                        if (!urlMatch.IsNullOrEmpty())
                        {
                            return urlMatch;
                        }
                    }
                }
            }


            string Name = Regex.Replace(game.Name, @"([ ]demo\b)", string.Empty, RegexOptions.IgnoreCase);
            Name = Regex.Replace(Name, @"(demo[ ])", string.Empty, RegexOptions.IgnoreCase);
            Name = CommonPluginsShared.PlayniteTools.NormalizeGameName(Name);

            url = string.Empty;
            url = UrlPCGamingWikiSearch + WebUtility.UrlEncode(Name);

            Thread.Sleep(1000);
            WebResponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
            if (!WebResponse.ToLower().Contains("search results"))
            {
                return url;
            }
            else
            {
                urlMatch = GetUrlIsOneResult(WebResponse);
                if (!urlMatch.IsNullOrEmpty())
                {
                    return urlMatch;
                }
            }


            url = string.Empty;
            url = UrlPCGamingWikiSearch + WebUtility.UrlEncode(game.Name);

            Thread.Sleep(1000);
            WebResponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
            if (!WebResponse.ToLower().Contains("search results"))
            {
                return url;
            }
            else
            {
                urlMatch = GetUrlIsOneResult(WebResponse);
                if (!urlMatch.IsNullOrEmpty())
                {
                    return urlMatch;
                }
            }


            return string.Empty;
        }


        public List<Localization> GetLocalizations(Game game)
        {
            string urlPCGamingWiki = string.Empty;
            int SteamId = 0;

            if (game.SourceId != default(Guid))
            {
                if (game.Source.Name.ToLower() == "steam")
                {
                    SteamId = int.Parse(game.GameId);
                }
            }
            if (SteamId == 0)
            {
                SteamId = steamApi.GetSteamId(game.Name);
            }

            urlPCGamingWiki = FindGoodUrl(game, SteamId);

            List<Localization> Localizations = new List<Localization>();

            if (SteamId != 0)
            {
                Localizations = GetLocalizations(string.Format(urlSteamId, SteamId));
                if (Localizations.Count > 0)
                {
                    return Localizations;
                }
            }
            if (!urlPCGamingWiki.IsNullOrEmpty())
            {
                Localizations = GetLocalizations(urlPCGamingWiki);
                if (Localizations.Count > 0)
                {
                    return Localizations;
                }
            }

            logger.Warn($"Not find for {game.Name}");
            return Localizations;
        }

        private List<Localization> GetLocalizations(string url)
        {
            List<Localization> Localizations = new List<Localization>();

            try
            {
                Common.LogDebug(true, $"url {url}");

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


                    Notes = Regex.Replace(Notes, "(</[^>]*>)", string.Empty);
                    Notes = Regex.Replace(Notes, "(<[^>]*>)", string.Empty);


                    Localizations.Add(new Models.Localization
                    {
                        Language = Language,
                        Ui = Ui,
                        Audio = Audio,
                        Sub = Sub,
                        Notes = WebUtility.HtmlDecode(Notes),
                        IsManual = false
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return Localizations;
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
