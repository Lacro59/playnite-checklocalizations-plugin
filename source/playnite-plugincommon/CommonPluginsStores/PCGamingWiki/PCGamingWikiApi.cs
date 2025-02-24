using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CommonPluginsShared;
using FuzzySharp;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CommonPluginsStores.PCGamingWiki
{
    public class PCGamingWikiApi
    {
        internal static ILogger Logger => LogManager.GetLogger();
        internal static IResourceProvider ResourceProvider => new ResourceProvider();

        internal string PluginName { get; set; }
        internal string ClientName => "PCGamingWiki";


        #region Url
        private string UrlBase => @"https://pcgamingwiki.com";
        private string UrlWithSteamId => UrlBase + @"/api/appid.php?appid={0}";
        private string UrlPCGamingWikiSearch => UrlBase + @"/w/index.php?search=";
        private string UrlPCGamingWikiSearchWithApi => UrlBase + @"/w/api.php?action=opensearch&format=json&formatversion=2&search={0}&namespace=0&limit=10";
        #endregion


        public PCGamingWikiApi(string pluginName)
        {
            PluginName = pluginName;
        }


        public string FindGoodUrl(Game game, uint steamId = 0)
        {
            string url = string.Empty;
            string urlMatch = string.Empty;
            string webResponse = string.Empty;

            if (steamId != 0)
            {
                url = string.Format(UrlWithSteamId, steamId);

                Thread.Sleep(500);
                webResponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
                if (!webResponse.ToLower().Contains("search results"))
                {
                    return url;
                }
            }

            if (game.Links != null)
            {
                foreach (Link link in game.Links)
                {
                    if (link.Url.ToLower().Contains("pcgamingwiki") && !link.Url.ToLower().StartsWith(@"http://pcgamingwiki.com/w/index.php?search="))
                    {
                        return link.Url;
                    }
                }
            }

            string Name = Regex.Replace(game.Name, @"([ ]demo\b)", string.Empty, RegexOptions.IgnoreCase);
            Name = Regex.Replace(Name, @"(demo[ ])", string.Empty, RegexOptions.IgnoreCase);
            Name = CommonPluginsShared.PlayniteTools.NormalizeGameName(Name);


            // Search with release date
            if (game.ReleaseDate != null)
            {
                url = string.Format(UrlPCGamingWikiSearchWithApi, WebUtility.UrlEncode(Name + $" ({((ReleaseDate)game.ReleaseDate).Year})"));
                urlMatch = GetWithSearchApi(url);
                if (!urlMatch.IsNullOrEmpty())
                {
                    return urlMatch;
                }
            }

            // Normal search
            url = string.Format(UrlPCGamingWikiSearchWithApi, WebUtility.UrlEncode(Name));
            urlMatch = GetWithSearchApi(url);
            if (!urlMatch.IsNullOrEmpty())
            {
                return urlMatch;
            }

            return string.Empty;
        }


        private string GetWithSearchApi(string url)
        {
            string urlFound = string.Empty;

            try
            {
                string WebResponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
                if (Serialization.TryFromJson(WebResponse, out dynamic data) && data[3]?.Count > 0)
                {
                    List<string> listName = Serialization.FromJson<List<string>>(Serialization.ToJson(data[1]));
                    List<string> listUrl = Serialization.FromJson<List<string>>(Serialization.ToJson(data[3]));

                    Dictionary<string, string> dataFound = new Dictionary<string, string>();
                    for (int i = 0; i < listName.Count; i++)
                    {
                        dataFound.Add(listName[i], listUrl[i]);
                    }

                    var fuzzList = dataFound.Select(x => new { MatchPercent = Fuzz.Ratio(data[0].ToString().ToLower(), x.Key.ToLower()), Data = x })
                        .OrderByDescending(x => x.MatchPercent)
                        .ToList();

                    urlFound = fuzzList.First().MatchPercent >= 95 ? fuzzList.First().Data.Value : string.Empty;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return urlFound;
        }
    }
}