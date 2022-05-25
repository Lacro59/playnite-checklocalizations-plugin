using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared;
using CommonPluginsStores.PCGamingWiki;
using CommonPluginsStores.Steam;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace CheckLocalizations.Clients
{
    public class PCGamingWikiLocalizations
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;


        private SteamApi _steamApi;
        internal SteamApi steamApi
        {
            get
            {
                if (_steamApi == null)
                {
                    _steamApi = new SteamApi(PluginDatabase.PluginName);
                }
                return _steamApi;
            }

            set => _steamApi = value;
        }

        private PCGamingWikiApi _pcGamingWikiApi;
        internal PCGamingWikiApi pcGamingWikiApi
        {
            get
            {
                if (_pcGamingWikiApi == null)
                {
                    _pcGamingWikiApi = new PCGamingWikiApi(PluginDatabase.PluginName);
                }
                return _pcGamingWikiApi;
            }

            set => _pcGamingWikiApi = value;
        }


        private string gamePCGamingWiki = string.Empty;
        private string urlPCGamingWiki = string.Empty;


        public PCGamingWikiLocalizations()
        {

        }



        public string GetUrl()
        {
            return urlPCGamingWiki;
        }

        public string GetGameName()
        {
            return gamePCGamingWiki;
        }


        public List<Localization> GetLocalizations(Game game)
        {
            urlPCGamingWiki = string.Empty;
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
                SteamId = steamApi.GetAppId(game.Name);
            }

            urlPCGamingWiki = pcGamingWikiApi.FindGoodUrl(game, SteamId);

            List<Localization> Localizations = new List<Localization>();
            if (!urlPCGamingWiki.IsNullOrEmpty())
            {
                Localizations = GetLocalizations(urlPCGamingWiki);
                if (Localizations.Count > 0)
                {
                    return Localizations;
                }
            }

            logger.Warn($"Not find localizations for {game.Name}");
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


                gamePCGamingWiki = HtmlLocalization.QuerySelector("h1.article-title")?.InnerHtml;


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
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
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
