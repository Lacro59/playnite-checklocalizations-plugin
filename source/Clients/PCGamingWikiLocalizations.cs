using AngleSharp.Dom;
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
        private static ILogger Logger => LogManager.GetLogger();

        private static LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;


        private PCGamingWikiApi _pcGamingWikiApi;
        internal PCGamingWikiApi PCGamingWikiApi
        {
            get
            {
                if (_pcGamingWikiApi == null)
                {
                    _pcGamingWikiApi = new PCGamingWikiApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckDlc);
                }
                return _pcGamingWikiApi;
            }

            set => _pcGamingWikiApi = value;
        }


        private string GamePCGamingWiki { get; set; } = string.Empty;
        private string UrlPCGamingWiki { get; set; } = string.Empty;


        private static Dictionary<string, SupportStatus> SupportStatusMap => new Dictionary<string, SupportStatus>
        {
            { "native support", SupportStatus.Native },
            { "no native support", SupportStatus.NoNative },
            { "hackable", SupportStatus.Hackable },
            { "not applicable", SupportStatus.NotApplicable }
        };


        public PCGamingWikiLocalizations()
        {

        }



        public string GetUrl()
        {
            return UrlPCGamingWiki;
        }

        public string GetGameName()
        {
            return GamePCGamingWiki;
        }


        public List<Localization> GetLocalizations(Game game)
        {
            List<Localization> Localizations = new List<Localization>();
            UrlPCGamingWiki = PCGamingWikiApi.FindGoodUrl(game);

            if (!UrlPCGamingWiki.IsNullOrEmpty())
            {
                Localizations = GetLocalizations(UrlPCGamingWiki);
                if (Localizations.Count > 0)
                {
                    return Localizations;
                }
            }

            Logger.Warn($"Not find localizations for {game.Name}");
            return Localizations;
        }

        private List<Localization> GetLocalizations(string url)
        {
            List<Localization> Localizations = new List<Localization>();

            try
            {
                string response = Web.DownloadStringData(url).GetAwaiter().GetResult();
                HtmlParser parser = new HtmlParser();
                IHtmlDocument htmlLocalization = parser.Parse(response);

                GamePCGamingWiki = htmlLocalization.QuerySelector("h1.article-title")?.InnerHtml;

                foreach (IElement row in htmlLocalization.QuerySelectorAll("tr.table-l10n-body-row"))
                {
                    string language = Regex.Replace(row.QuerySelector("th").InnerHtml, "<.+?>(.*)<.+?>", "$1");
                    SupportStatus ui = SupportStatus.Unknown;
                    SupportStatus audio = SupportStatus.Unknown;
                    SupportStatus sub = SupportStatus.Unknown;
                    string notes = string.Empty;

                    int i = 1;
                    foreach (IElement td in row.QuerySelectorAll("td"))
                    {
                        switch (i)
                        {
                            case 1:
                                ui = GetSupportStatus(td.QuerySelector("div").GetAttribute("title"));
                                break;
                            case 2:
                                audio = GetSupportStatus(td.QuerySelector("div").GetAttribute("title"));
                                break;
                            case 3:
                                sub = GetSupportStatus(td.QuerySelector("div").GetAttribute("title"));
                                break;
                            case 4:
                                notes = Regex.Replace(td.InnerHtml, "<.+?>(.*)<.+?>", "$1");
                                break;
                            default:
                                break;
                        }
                        i++;
                    }

                    notes = Regex.Replace(notes, "(</[^>]*>)", string.Empty);
                    notes = Regex.Replace(notes, "(<[^>]*>)", string.Empty);

                    Localizations.Add(new Localization
                    {
                        Language = language,
                        Ui = ui,
                        Audio = audio,
                        Sub = sub,
                        Notes = WebUtility.HtmlDecode(notes),
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
            return SupportStatusMap.TryGetValue(title.ToLower(), out SupportStatus status) ? status : SupportStatus.Unknown;
        }
    }
}
