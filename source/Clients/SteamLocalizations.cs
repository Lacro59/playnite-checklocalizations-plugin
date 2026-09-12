using CheckLocalizations.Models;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsStores.Steam.Models.SteamKit;
using Playnite.SDK.Data;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommonPlayniteShared.Common;
using CheckLocalizations.Services;
using CommonPluginsStores.Steam;
using CommonPluginsStores.Models;
using static CommonPluginsShared.PlayniteTools;

namespace CheckLocalizations.Clients
{
    public class SteamLocalizations
    {
        private static ILogger Logger => LogManager.GetLogger();

        private static LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;

        private SteamApi SteamApi { get; set; }
        private uint AppId { get; set; }

        private List<SteamApp> steamApps;
        private string AppsListPath => Path.Combine(SteamApi.PathStoresData, "Steam_AppsList.json");
        private const string UrlSteamAppIdListGames = "https://raw.githubusercontent.com/jsnli/steamappidlist/master/data/games_appid.json";
        private const string UrlSteamAppIdListDlc = "https://raw.githubusercontent.com/jsnli/steamappidlist/master/data/dlc_appid.json";

        private List<SteamApp> GetPublicSteamApps()
        {
            if (steamApps != null)
            {
                return steamApps;
            }

            try
            {
                // Reuse SteamApi's app-list cache and its three-day lifetime.
                if (File.Exists(AppsListPath) && File.GetLastWriteTime(AppsListPath).AddDays(3) > DateTime.Now
                    && Serialization.TryFromJsonFile(AppsListPath, out List<SteamApp> cachedApps)
                    && cachedApps?.Count > 0)
                {
                    var cachedAppIds = new HashSet<uint>();
                    steamApps = cachedApps
                        .Where(app => app != null && cachedAppIds.Add(app.AppId))
                        .OrderBy(app => app.AppId).ToList();
                    if (steamApps.Count > 0)
                    {
                        return steamApps;
                    }
                    steamApps = null;
                }

                string gamesJson = Web.DownloadStringData(UrlSteamAppIdListGames).GetAwaiter().GetResult();
                string dlcJson = Web.DownloadStringData(UrlSteamAppIdListDlc).GetAwaiter().GetResult();
                List<SteamApp> games = string.IsNullOrWhiteSpace(gamesJson)
                    ? new List<SteamApp>()
                    : Serialization.FromJson<List<SteamApp>>(gamesJson) ?? new List<SteamApp>();
                List<SteamApp> dlcs = string.IsNullOrWhiteSpace(dlcJson)
                    ? new List<SteamApp>()
                    : Serialization.FromJson<List<SteamApp>>(dlcJson) ?? new List<SteamApp>();
                var seenAppIds = new HashSet<uint>();
                List<SteamApp> merged = games.Concat(dlcs)
                    .Where(app => app != null && seenAppIds.Add(app.AppId))
                    .OrderBy(app => app.AppId).ToList();
                if (merged.Count > 0)
                {
                    steamApps = merged;
                    FileSystem.WriteStringToFileSafe(AppsListPath, Serialization.ToJson(steamApps));
                }
                return merged;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
                return steamApps ?? new List<SteamApp>();
            }
        }

        // Keep the link parsing behavior of SteamApi.GetAppIdFromLinks(Game).
        private uint GetAppIdFromLinks(Game game)
        {
            Link steamLink = game.Links?.FirstOrDefault(link => link.Name.ToLower() == "steam");
            if (steamLink == null)
            {
                return 0;
            }

            string[] linkSplit = steamLink.Url.Split(new[] { "/app/" }, StringSplitOptions.None);
            string steamIdString = linkSplit?.ElementAtOrDefault(1)?.Split('/').FirstOrDefault();
            if (steamIdString == null)
            {
                return 0;
            }

            bool success = uint.TryParse(steamIdString, out uint steamId);
            return !success ? 0 : steamId;
        }

        private uint GetAppId(Game game)
        {
            uint appIdFromLinks = GetAppIdFromLinks(game);
            if (appIdFromLinks != 0)
            {
                return appIdFromLinks;
            }

            List<uint> found = GetPublicSteamApps()
                .Where(app => app.Name.IsEqual(game.Name, true))
                .Select(app => app.AppId).ToList();
            if (found.Count > 1)
            {
                Logger.Warn($"Found {found.Count} SteamAppId data for {game.Name}: " + string.Join(", ", found));
                return 0;
            }

            return found.FirstOrDefault();
        }


        public SteamLocalizations()
        {
            SteamApi = new SteamApi(PluginDatabase.PluginName, ExternalPlugin.CheckLocalizations);
        }


        public string GetUrl()
        {
            return $"https://store.steampowered.com/app/{AppId}/";
        }

        public string GetGameName()
        {
            return SteamApi.GetGameName(AppId);
        }


        public List<Localization> GetLocalizations(Game game)
        {
            List<Localization> localizations = new List<Localization>();

            try
            {
                AppId = GetAppId(game);
                if (AppId != 0)
                {
                    GameInfos gameInfos = SteamApi.GetGameInfos(AppId.ToString(), null);
                    if (gameInfos == null)
                    {
                        Logger.Warn($"Error retrieving Steam data from {game.Name} - {AppId}");
                        return localizations;
                    }

                    string[] dataSplited = gameInfos.Languages.Split(new string[] { "<br>" }, StringSplitOptions.None);
                    string[] listLocalizations = dataSplited[0].Split(',');

                    foreach(string localization in listLocalizations)
                    {
                        string language = string.Empty;
                        SupportStatus ui = SupportStatus.Native;
                        SupportStatus audio = SupportStatus.Unknown;
                        SupportStatus sub = SupportStatus.Unknown;

                        if (localization.Contains("<strong>*</strong>"))
                        {
                            audio = SupportStatus.Native;
                            sub = SupportStatus.Native;
                        }
                            
                        language = localization.Replace("<strong>*</strong>", string.Empty).Trim();
                        switch (language)
                        {
                            case "Portuguese - Brazil":
                                language = "Brazilian Portuguese";
                                break;
                            case "Spanish - Spain":
                                language = "Spanish";
                                break;
                            default:
                                break;
                        }

                        localizations.Add(new Localization
                        {
                            Language = language,
                            Ui = ui,
                            Audio = audio,
                            Sub = sub,
                            Notes = string.Empty,
                            IsManual = false
                        });
                    }
                }
                else
                {
                    Logger.Warn($"Not find for {game.Name}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error with {game.Name} - {AppId}", true, PluginDatabase.PluginName);
            }

            return localizations;
        }
    }
}
