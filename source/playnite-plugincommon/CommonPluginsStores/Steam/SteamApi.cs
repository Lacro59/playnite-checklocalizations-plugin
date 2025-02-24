using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsStores.Models;
using CommonPluginsStores.Steam.Models;
using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Data;
using SteamKit2;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommonPlayniteShared;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using CommonPlayniteShared.Common;
using System.Security.Principal;
using System.Threading.Tasks;
using CommonPluginsShared.Converters;
using System.Globalization;
using System.Windows.Media;
using System.Text.RegularExpressions;
using CommonPlayniteShared.PluginLibrary.SteamLibrary.SteamShared;
using System.Net;
using static CommonPluginsShared.PlayniteTools;
using System.Net.Http;
using System.Windows;
using System.Threading;
using CommonPluginsStores.Steam.Models.SteamKit;
using Playnite.SDK.Models;

namespace CommonPluginsStores.Steam
{
    public class SteamApi : StoreApi
    {
        #region Url
        private static string SteamDbDlc => "https://steamdb.info/app/{0}/dlc/";
        private static string SteamDbExtensionAchievements => "https://steamdb.info/api/ExtensionGetAchievements/?appid={0}";

        private static string UrlCapsuleSteam => "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{0}/capsule_184x69.jpg";

        private static string UrlSteamCommunity => @"https://steamcommunity.com";
        private static string UrlApi => @"https://api.steampowered.com";
        private static string UrlStore => @"https://store.steampowered.com";

        private static string UrlAvatarFul => @"https://avatars.akamai.steamstatic.com/{0}_full.jpg";

        private static string UrlWishlistApi = UrlApi + @"/IWishlistService/GetWishlist/v1?steamid={0}&key={1}";

        private static string UrlLogin => UrlSteamCommunity + @"/login/home/?goto=";
        private static string UrlProfileById => UrlSteamCommunity + @"/profiles/{0}";
        private static string UrlProfileByName => UrlSteamCommunity + @"/id/{0}";
        private static string UrlFriends => UrlSteamCommunity + @"/profiles/{0}/friends";

        private static string UrlAccount => UrlStore + @"/account";
        private static string UrlUserData => UrlStore + @"/dynamicstore/userdata/";
        private static string UrlWishlist => UrlStore + @"/wishlist/profiles/{0}/";
        private static string UrlWishlistRemove => UrlStore + @"/api/removefromwishlist";

        private static string UrlApiGameDetails => UrlStore + @"/api/appdetails?appids={0}&l={1}";
        private static string UrlSteamGame => UrlStore + @"/app/{0}";
        private static string UrlSteamGameLocalised => UrlStore + @"/app/{0}/?l={1}";
        #endregion

        protected List<SteamApp> steamApps;
        internal List<SteamApp> SteamApps
        {
            get
            {
                if (steamApps == null)
                {
                    // From cache if exists & not expired
                    if (File.Exists(AppsListPath) && File.GetLastWriteTime(AppsListPath).AddDays(3) > DateTime.Now)
                    {
                        Common.LogDebug(true, "GetSteamAppListFromCache");
                        _ = Serialization.TryFromJsonFile(AppsListPath, out steamApps);
                    }
                    // From web
                    else
                    {
                        Common.LogDebug(true, "GetSteamAppsListFromWeb");
                        steamApps = SteamKit.GetAppList();
                        if (steamApps?.Count > 0)
                        {
                            FileSystem.WriteStringToFileSafe(AppsListPath, Serialization.ToJson(steamApps));
                        }
                    }
                }
                return steamApps;
            }

            set => steamApps = value;
        }

        private SteamUserData UserData => LoadUserData() ?? GetUserData() ?? LoadUserData(false);

        #region Paths
        private string AppsListPath { get; }
        private string FileUserData { get; }

        private string installationPath;
        public string InstallationPath
        {
            get
            {
                if (installationPath == null)
                {
                    installationPath = GetInstallationPath();
                }
                return installationPath;
            }

            set => SetValue(ref installationPath, value);
        }

        public string LoginUsersPath { get; }
        #endregion


        public SteamApi(string pluginName, ExternalPlugin pluginLibrary) : base(pluginName, pluginLibrary, "Steam")
        {
            AppsListPath = Path.Combine(PathStoresData, "Steam_AppsList.json");
            FileUserData = Path.Combine(PathStoresData, "Steam_UserData.json");

            LoginUsersPath = Path.Combine(InstallationPath, "config", "loginusers.vdf");

            CookiesDomains = new List<string>
            {
                ".steamcommunity.com", "steamcommunity.com", "steampowered.com", "store.steampowered.com", "help.steampowered.com", "login.steampowered.com"
            };
        }


        #region Configuration
        protected override bool GetIsUserLoggedIn()
        {
            if (CurrentAccountInfos == null)
            {
                return false;
            }

            if (StoreSettings.UseAuth)
            {
                SteamUserData userData = GetUserData();
                return userData?.RgOwnedApps?.Count > 0;
            }

            Task<bool> withId = IsProfilePublic(string.Format(UrlProfileById, CurrentAccountInfos.UserId), GetStoredCookies());
            Task<bool> withPersona = IsProfilePublic(string.Format(UrlProfileByName, CurrentAccountInfos.Pseudo), GetStoredCookies());
            Task.WaitAll(withId, withPersona);

            return withId.Result || withPersona.Result;
        }

        public override void Login()
        {
            ResetIsUserLoggedIn();
            string steamId = string.Empty;
            using (IWebView view = API.Instance.WebViews.CreateView(675, 440, Colors.Black))
            {
                view.LoadingChanged += async (s, e) =>
                {
                    string address = view.GetCurrentAddress();
                    if (address.Contains(@"steamcommunity.com"))
                    {
                        string source = await view.GetPageSourceAsync();
                        Match idMatch = Regex.Match(source, @"g_steamID = ""(\d+)""");
                        if (idMatch.Success)
                        {
                            steamId = idMatch.Groups[1].Value;
                        }
                        else
                        {
                            idMatch = Regex.Match(source, @"steamid"":""(\d+)""");
                            if (idMatch.Success)
                            {
                                steamId = idMatch.Groups[1].Value;
                            }
                        }

                        if (idMatch.Success)
                        {
                            _ = SetStoredCookies(GetWebCookies());
                            string JsonDataString = Tools.GetJsonInString(source, @"(?<=g_rgProfileData[ ]=[ ])");
                            RgProfileData rgProfileData = Serialization.FromJson<RgProfileData>(JsonDataString);

                            CurrentAccountInfos = new AccountInfos
                            {
                                UserId = rgProfileData.SteamId.ToString(),
                                Avatar = string.Format(UrlAvatarFul, steamId),
                                Pseudo = rgProfileData.PersonaName,
                                Link = rgProfileData.Url,
                                IsPrivate = true,
                                IsCurrent = true
                            };
                            SaveCurrentUser();
                            _ = GetCurrentAccountInfos();

                            Logger.Info($"{PluginName} logged");

                            view.Close();
                        }
                    }
                };

                CookiesDomains.ForEach(x =>
                {
                    view.DeleteDomainCookies(x);
                });

                view.Navigate(UrlLogin);
                _ = view.OpenDialog();
            }
        }

        public bool IsConfigured()
        {
            if (CurrentAccountInfos == null)
            {
                return false;
            }

            string SteamId = CurrentAccountInfos.UserId;
            string SteamUser = CurrentAccountInfos.Pseudo;

            return !SteamId.IsNullOrEmpty() && !SteamUser.IsNullOrEmpty();
        }
        #endregion


        #region Current user
        protected override AccountInfos GetCurrentAccountInfos()
        {
            AccountInfos accountInfos = LoadCurrentUser();
            if (accountInfos != null)
            {
                _ = Task.Run(() =>
                {
                    Thread.Sleep(1000);

                    if (CurrentAccountInfos.ApiKey.IsNullOrEmpty())
                    {
                        string response = Web.DownloadStringData(string.Format(UrlProfileById, accountInfos.UserId), GetStoredCookies()).GetAwaiter().GetResult();
                        string jsonDataString = Tools.GetJsonInString(response, @"(?<=g_rgProfileData[ ]=[ ])");
                        if (jsonDataString.Length < 5)
                        {
                            response = Web.DownloadStringData(string.Format(UrlProfileByName, accountInfos.Pseudo), GetStoredCookies()).GetAwaiter().GetResult();
                            jsonDataString = Tools.GetJsonInString(response, @"g_rgProfileData[ ]?=[ ]?");
                        }
                        _ = Serialization.TryFromJson(jsonDataString, out RgProfileData rgProfileData);

                        Match matches = Regex.Match(response, @"<link\srel=""image_src""\shref=""([^""]+)"">");
                        string avatar = matches.Success ? matches.Groups[1].Value : string.Format(UrlAvatarFul, accountInfos.UserId);

                        CurrentAccountInfos.Avatar = avatar;
                        CurrentAccountInfos.Pseudo = rgProfileData?.PersonaName ?? CurrentAccountInfos.Pseudo;
                        CurrentAccountInfos.Link = rgProfileData?.Url ?? CurrentAccountInfos.Link;
                    }
                    else if (ulong.TryParse(accountInfos.UserId, out ulong steamId))
                    {
                        ObservableCollection<AccountInfos> playerSummaries = GetPlayerSummaries(new List<ulong> { steamId });
                        CurrentAccountInfos.Avatar = playerSummaries?.FirstOrDefault().Avatar ?? CurrentAccountInfos.Avatar;
                        CurrentAccountInfos.Pseudo = playerSummaries?.FirstOrDefault().Pseudo ?? CurrentAccountInfos.Pseudo;
                        CurrentAccountInfos.Link = playerSummaries?.FirstOrDefault().Link ?? CurrentAccountInfos.Link;
                    }

                    CurrentAccountInfos.IsPrivate = !CheckIsPublic(accountInfos).GetAwaiter().GetResult();
                    CurrentAccountInfos.AccountStatus = CurrentAccountInfos.IsPrivate ? AccountStatus.Private : AccountStatus.Public;
                });
                return accountInfos;
            }
            return new AccountInfos { IsCurrent = true };
        }

        protected override ObservableCollection<AccountInfos> GetCurrentFriendsInfos()
        {
            try
            {
                ObservableCollection<AccountInfos> accountInfos = null;
                if (CurrentAccountInfos != null && CurrentAccountInfos.IsCurrent)
                {
                    accountInfos = StoreSettings.UseAuth || CurrentAccountInfos.IsPrivate || !StoreSettings.UseApi || CurrentAccountInfos.ApiKey.IsNullOrEmpty()
                        ? GetCurrentFriendsInfosByWeb()
                        : GetCurrentFriendsInfosByApi();
                }
                return accountInfos;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }
        #endregion

        #region User details
        // TODO
        public override ObservableCollection<AccountGameInfos> GetAccountGamesInfos(AccountInfos accountInfos)
        {
            try
            {
                //List<HttpHeader> httpHeaders = new List<HttpHeader>
                //{
                //    new HttpHeader { Key = "AuthToken", Value =  AuthToken.Token },
                //    new HttpHeader { Key = "Accept", Value = "application/json" }
                //};
                //string Url = string.Format(UrlApi3UserGames, CurrentAccountInfos.UserId, accountInfos.UserId);
                //string WebData = Web.DownloadStringData(Url, httpHeaders).GetAwaiter().GetResult();
                //Serialization.TryFromJson(WebData, out ProductInfosResponse productInfosResponse);
                //
                //ObservableCollection<AccountGameInfos> accountGamesInfos = new ObservableCollection<AccountGameInfos>();
                //productInfosResponse?.productInfos?.ForEach(x =>
                //{
                //    string Id = x.productId;
                //    string Name = x.displayProductName;
                //
                //    bool IsCommun = false;
                //    if (!accountInfos.IsCurrent)
                //    {
                //        IsCommun = CurrentGamesInfos?.Where(y => y.Id.IsEqual(Id))?.Count() != 0;
                //    }
                //
                //    GameInfos gameInfos = GetGameInfos(Id, Local);
                //    string Link = gameInfos?.Link;
                //
                //    string achId = x?.softwares?.softwareList?.First().achievementSetOverride;
                //    ObservableCollection<GameAchievement> Achievements = null;
                //    if (!achId.IsNullOrEmpty())
                //    {
                //        Achievements = GetAchievements(achId, accountInfos);
                //    }
                //
                //    AccountGameInfos accountGameInfos = new AccountGameInfos
                //    {
                //        Id = Id,
                //        Name = Name,
                //        Link = Link,
                //        IsCommun = IsCommun,
                //        Achievements = Achievements,
                //        HoursPlayed = 0
                //    };
                //    accountGamesInfos.Add(accountGameInfos);
                //});
                //
                //return accountGamesInfos;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }

        public override ObservableCollection<GameAchievement> GetAchievements(string id, AccountInfos accountInfos)
        {
            try
            {
                if (!uint.TryParse(id, out uint appId))
                {
                    return new ObservableCollection<GameAchievement>();
                }

                ObservableCollection<GameAchievement> gameAchievements = GetAchievementsSchema(appId);
                if (gameAchievements == null || gameAchievements.Count() == 0)
                {
                    return gameAchievements;
                }

                if (accountInfos?.IsCurrent ?? false)
                {
                    gameAchievements = StoreSettings.UseAuth || accountInfos.IsPrivate || !StoreSettings.UseApi || accountInfos.ApiKey.IsNullOrEmpty()
                        ? GetAchievementsByWeb(appId, accountInfos, gameAchievements)
                        : GetAchievementsByApi(appId, accountInfos, gameAchievements);
                }

                gameAchievements?.ForEach(x =>
                {
                    x.GamerScore = CalcGamerScore(x.Percent);
                });

                //gameAchievements = SetExtensionsAchievementsFromSteamDb(appId, gameAchievements);

                return gameAchievements;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }


        public override ObservableCollection<AccountWishlist> GetWishlist(AccountInfos accountInfos)
        {
            ObservableCollection<AccountWishlist> accountWishlists = new ObservableCollection<AccountWishlist>();
            if (accountInfos != null)
            {
                accountWishlists = StoreSettings.UseAuth || accountInfos.IsPrivate || !StoreSettings.UseApi || accountInfos.ApiKey.IsNullOrEmpty()
                    ? GetWishlistByWeb(accountInfos)
                    : GetWishlistByApi(accountInfos);
            }

            return accountWishlists;
        }

        public override bool RemoveWishlist(string id)
        {
            if (IsUserLoggedIn)
            {
                try
                {
                    List<HttpCookie> cookies = GetStoredCookies();
                    string sessionid = cookies?.FirstOrDefault(x => x.Name.IsEqual("sessionid"))?.Value;
                    if (sessionid.IsNullOrEmpty())
                    {
                        Logger.Warn($"Steam is not authenticate");
                        API.Instance.Notifications.Add(new NotificationMessage(
                            $"{PluginName}-steam-notlogged",
                            $"{PluginName}" + Environment.NewLine + ResourceProvider.GetString("LOCSteamNotLoggedIn"),
                            NotificationType.Error
                        ));
                    }

                    string url = string.Format(UrlWishlistRemove, CurrentAccountInfos.UserId);

                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "sessionid", sessionid },
                        { "appid", id }
                    };
                    FormUrlEncodedContent formContent = new FormUrlEncodedContent(data);

                    string response = Web.PostStringDataCookies(url, formContent, cookies).GetAwaiter().GetResult();
                    return response.IndexOf("{\"success\":true") > -1;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error remove {id} in {ClientName} wishlist", true, PluginName);
                }
            }

            return false;
        }
        #endregion


        #region Game
        /// <summary>
        /// Get game informations.
        /// </summary>
        /// <param name="id">appId</param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public override GameInfos GetGameInfos(string id, AccountInfos accountInfos)
        {
            return GetGameInfos(id, accountInfos, 1);
        }

        private GameInfos GetGameInfos(string id, AccountInfos accountInfos, int retryCount)
        {
            try
            {
                Thread.Sleep(2000); // Prevent http 429
                string url = string.Format(UrlApiGameDetails, id, CodeLang.GetSteamLang(Local));
                string webData = Web.DownloadStringData(url).GetAwaiter().GetResult();

                if (Serialization.TryFromJson(webData, out Dictionary<string, StoreAppDetailsResult> parsedData))
                {
                    StoreAppDetailsResult storeAppDetailsResult = parsedData[id];
                    if (storeAppDetailsResult?.data == null)
                    {
                        return null;
                    }

                    string format = "d MMM, yyyy";
                    CultureInfo culture = CultureInfo.InvariantCulture;
                    string dateString = storeAppDetailsResult?.data?.release_date?.date;
                    DateTime? released = null;
                    if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out DateTime releasedDate))
                    {
                        released = releasedDate;
                    }

                    GameInfos gameInfos = new GameInfos
                    {
                        Id = storeAppDetailsResult?.data.steam_appid.ToString(),
                        Name = storeAppDetailsResult?.data.name,
                        Link = string.Format(UrlSteamGameLocalised, id, CodeLang.GetSteamLang(Local)),
                        Image = storeAppDetailsResult?.data.header_image,
                        Description = ParseDescription(storeAppDetailsResult?.data.about_the_game),
                        Languages = storeAppDetailsResult?.data.supported_languages,
                        Released = released
                    };

                    // DLC
                    List<uint> dlcsIdSteam = storeAppDetailsResult?.data.dlc ?? new List<uint>();
                    List<uint> dlcsIdSteamDb = new List<uint>(); // GetDlcFromSteamDb(storeAppDetailsResult?.data.steam_appid ?? 0);
                    List<uint> dlcsId = dlcsIdSteam.Union(dlcsIdSteamDb).Distinct().OrderBy(x => x).ToList();

                    if (dlcsId.Count > 0)
                    {
                        ObservableCollection<DlcInfos> Dlcs = GetDlcInfos(dlcsId, accountInfos);
                        gameInfos.Dlcs = Dlcs;
                    }

                    return gameInfos;
                }
                else
                {
                    if (webData.Length < 25 && retryCount < 11)
                    {
                        Thread.Sleep(20000 * retryCount);
                        Logger.Warn($"Api limit for Steam with {id} - {retryCount}");
                        retryCount++;
                        return GetGameInfos(id, accountInfos, retryCount);
                    }
                    else if (retryCount == 10)
                    {
                        Logger.Warn($"Api limit exced for Steam with {id}");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }

        public override ObservableCollection<DlcInfos> GetDlcInfos(string id, AccountInfos accountInfos)
        {
            GameInfos gameInfos = GetGameInfos(id, accountInfos);
            return gameInfos?.Dlcs ?? new ObservableCollection<DlcInfos>();
        }

        public ObservableCollection<DlcInfos> GetDlcInfos(List<uint> dlcs, AccountInfos accountInfos)
        {
            try
            {
                ObservableCollection<DlcInfos> Dlcs = new ObservableCollection<DlcInfos>();
                dlcs.ForEach(x =>
                {
                    string Url = string.Format(UrlApiGameDetails, x, CodeLang.GetSteamLang(Local));
                    string WebData = Web.DownloadStringData(Url).GetAwaiter().GetResult();

                    if (Serialization.TryFromJson(WebData, out Dictionary<string, StoreAppDetailsResult> parsedData))
                    {
                        StoreAppDetailsResult storeAppDetailsResult = parsedData[x.ToString()];
                        if (storeAppDetailsResult?.data != null)
                        {
                            bool IsOwned = false;
                            if (accountInfos != null && accountInfos.IsCurrent)
                            {
                                IsOwned = IsDlcOwned(storeAppDetailsResult?.data.steam_appid.ToString());
                            }

                            DlcInfos dlc = new DlcInfos
                            {
                                Id = storeAppDetailsResult.data.steam_appid.ToString(),
                                Name = storeAppDetailsResult.data.name,
                                Description = ParseDescription(storeAppDetailsResult?.data.about_the_game),
                                Image = storeAppDetailsResult.data.header_image,
                                Link = string.Format(UrlSteamGameLocalised, storeAppDetailsResult.data.steam_appid.ToString(), CodeLang.GetSteamLang(Local)),
                                IsOwned = IsOwned,
                                Price = storeAppDetailsResult.data.is_free ? "0" : storeAppDetailsResult.data.price_overview?.final_formatted,
                                PriceBase = storeAppDetailsResult.data.is_free ? "0" : storeAppDetailsResult.data.price_overview?.initial_formatted
                            };

                            Dlcs.Add(dlc);
                        }
                    }
                });

                return Dlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }
        #endregion

        #region Games owned
        internal override ObservableCollection<GameDlcOwned> GetGamesDlcsOwned()
        {
            if (!IsUserLoggedIn)
            {
                return null;
            }

            try
            {
                ObservableCollection<GameDlcOwned> GamesDlcsOwned = new ObservableCollection<GameDlcOwned>();
                UserData?.RgOwnedApps?.ForEach(x =>
                {
                    GamesDlcsOwned.Add(new GameDlcOwned { Id = x.ToString() });
                });
                return GamesDlcsOwned;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }
        #endregion


        #region Steam
        /// <summary>
        /// Get AppId from Steam store with a game.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public uint GetAppId(Game game)
        {
            uint appIdFromLinks = GetAppIdFromLinks(game);
            if (appIdFromLinks != 0)
            {
                return appIdFromLinks;
            }

            if (SteamApps == null)
            {
                Logger.Warn("SteamApps is empty");
                return 0;
            }

            SteamApps.Sort((x, y) => x.AppId.CompareTo(y.AppId));
            List<uint> found = SteamApps.FindAll(x => x.Name.IsEqual(game.Name, true)).Select(x => x.AppId).Distinct().ToList();

            if (found != null && found.Count > 0)
            {
                if (found.Count > 1)
                {
                    Logger.Warn($"Found {found.Count} SteamAppId data for {game.Name}: " + string.Join(", ", found));
                    return 0;
                }

                Common.LogDebug(true, $"Found SteamAppId data for {game.Name} - {Serialization.ToJson(found)}");
                return found.First();
            }

            return 0;
        }

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

        /// <summary>
        /// Get name from Steam store with an appId.
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public string GetGameName(uint appId)
        {
            SteamApp found = SteamApps?.Find(x => x.AppId == appId);
            if (found != null)
            {
                Common.LogDebug(true, $"Found {ClientName} data for {appId} - {Serialization.ToJson(found)}");
                return found.Name;
            }
            else
            {
                Logger.Warn($"Not found {ClientName} data for {appId}");
            }
            return string.Empty;
        }


        /// <summary>
        /// Get AccountID for a SteamId
        /// </summary>
        /// <returns></returns>
        public static uint GetAccountId(ulong SteamId)
        {
            try
            {
                SteamID steamID = new SteamID();
                steamID.SetFromUInt64(SteamId);
                return steamID.AccountID;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
            return 0;
        }


        /// <summary>
        /// Get the Steam installation path.
        /// </summary>
        /// <returns></returns>
        public string GetInstallationPath()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key?.GetValueNames().Contains("SteamPath") == true)
                    {
                        return key.GetValue("SteamPath")?.ToString().Replace('/', '\\') ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            Logger.Warn($"No {ClientName} installation found");
            return string.Empty;
        }

        /// <summary>
        /// Get the Steam screanshots folder.
        /// </summary>
        /// <returns></returns>
        public string GetScreeshotsPath()
        {
            if (CurrentAccountInfos != null)
            {
                string PathScreeshotsFolder = Path.Combine(InstallationPath, "userdata", GetAccountId(ulong.Parse(CurrentAccountInfos.UserId)).ToString(), "760", "remote");
                if (Directory.Exists(PathScreeshotsFolder))
                {
                    return PathScreeshotsFolder;
                }
            }

            Logger.Warn($"No {ClientName} screenshots folder found");
            return string.Empty;
        }


        /// <summary>
        /// Get the list of all users defined in local.
        /// </summary>
        /// <returns></returns>
        public List<Models.SteamUser> GetSteamUsers()
        {
            List<Models.SteamUser> users = new List<Models.SteamUser>();
            if (File.Exists(LoginUsersPath))
            {
                KeyValue config = new KeyValue();
                try
                {
                    Stream sFile = FileSystem.OpenReadFileStreamSafe(LoginUsersPath);
                    _ = config.ReadAsText(sFile);
                    foreach (KeyValue user in config.Children)
                    {
                        users.Add(new Models.SteamUser()
                        {
                            SteamId = ulong.Parse(user.Name),
                            AccountName = user["AccountName"].Value,
                            PersonaName = user["PersonaName"].Value,
                        });
                    }
                    return users;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return null;
        }

        public List<SteamStats> GetUsersStats(uint appId, AccountInfos accountInfos)
        {
            return StoreSettings.UseAuth || accountInfos.IsPrivate || !StoreSettings.UseApi || accountInfos.ApiKey.IsNullOrEmpty()
                        ? new List<SteamStats>()
                        : SteamKit.GetUserStatsForGame(accountInfos.ApiKey, appId, ulong.Parse(accountInfos.UserId));
        }

        public async Task<bool> CheckIsPublic(Models.SteamUser steamUser)
        {
            bool withId = await IsProfilePublic(string.Format(UrlProfileById, steamUser.SteamId));
            bool withPersona = await IsProfilePublic(string.Format(UrlProfileByName, steamUser.PersonaName));

            return withId || withPersona;
        }

        public async Task<bool> CheckIsPublic(AccountInfos accountInfos)
        {
            return accountInfos == null || accountInfos.UserId.IsNullOrEmpty() || accountInfos.UserId == "0" || accountInfos.Pseudo.IsNullOrEmpty()
                ? false
                : await CheckIsPublic(new Models.SteamUser { SteamId = ulong.Parse(accountInfos.UserId), PersonaName = accountInfos.Pseudo });
        }

        private async Task<bool> IsProfilePublic(string profilePageUrl)
        {
            return await IsProfilePublic(profilePageUrl, null);
        }

        private async Task<bool> IsProfilePublic(string profilePageUrl, List<HttpCookie> httpCookies)
        {
            try
            {
                string ResultWeb = await Web.DownloadStringData(profilePageUrl, httpCookies);
                IHtmlDocument HtmlDoc = new HtmlParser().Parse(ResultWeb);
                IElement profile_private_info = HtmlDoc.QuerySelector("div.profile_private_info");
                IElement error_ctn = HtmlDoc.QuerySelector("div.error_ctn");

                return profile_private_info == null && error_ctn == null;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return false;
        }


        private SteamUserData LoadUserData(bool onlyNow = true)
        {
            if (File.Exists(FileUserData))
            {
                try
                {
                    DateTime dateLastWrite = File.GetLastWriteTime(FileUserData);
                    if (onlyNow && dateLastWrite.AddMinutes(5) <= DateTime.Now)
                    {
                        return null;
                    }

                    if (!onlyNow)
                    {
                        ShowNotificationOldData(dateLastWrite);
                    }

                    return Serialization.FromJsonFile<SteamUserData>(FileUserData);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return null;
        }

        private SteamUserData GetUserData()
        {
            try
            {
                List<HttpCookie> cookies = GetStoredCookies();
                string resultWeb = Web.DownloadStringData(UrlUserData, cookies, Web.UserAgent, true).GetAwaiter().GetResult();
                if (Serialization.TryFromJson(resultWeb, out SteamUserData userData, out Exception ex))
                {
                    _ = SetStoredCookies(GetNewWebCookies(UrlAccount));
                    SaveUserData(userData);
                }
                else
                {
                    Common.LogError(ex, false, false, PluginName);
                    userData = LoadUserData(false);
                }
                return userData;
            }
            catch (WebException ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private void SaveUserData(SteamUserData userData)
        {
            if (userData?.RgOwnedApps?.Count == 0)
            {
                return;
            }

            FileSystem.PrepareSaveFile(FileUserData);
            File.WriteAllText(FileUserData, Serialization.ToJson(userData));
        }


        internal string ParseDescription(string description)
        {
            return description.Replace("%CDN_HOST_MEDIA_SSL%", "steamcdn-a.akamaihd.net");
        }



        public bool CheckGameIsPrivate(uint appId, AccountInfos accountInfos)
        {
            return StoreSettings.UseAuth || accountInfos.IsPrivate || !StoreSettings.UseApi || accountInfos.ApiKey.IsNullOrEmpty()
                ? CheckGameIsPrivateByWeb(appId, accountInfos)
                : SteamKit.CheckGameIsPrivate(accountInfos.ApiKey, appId, ulong.Parse(accountInfos.UserId));
        }
        #endregion

        #region Steam Api
        /// <summary>
        /// Get game achievements schema with hidden description & percent & without stats
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public ObservableCollection<GameAchievement> GetAchievementsSchema(uint appId)
        {
            ObservableCollection<GameAchievement> gameAchievements = null;
            if (appId > 0)
            {
                List<Models.SteamKit.SteamAchievements> steamAchievements = SteamKit.GetGameAchievements(appId, CodeLang.GetSteamLang(Local));
                gameAchievements = steamAchievements?.Select(x => new GameAchievement
                {
                    Id = x.InternalName,
                    Name = x.LocalizedName,
                    Description = x.LocalizedDesc,
                    UrlUnlocked = x.Icon,
                    UrlLocked = x.IconGray,
                    DateUnlocked = default,
                    Percent = x.PlayerPercentUnlocked,
                    IsHidden = x.Hidden
                }).ToObservable();
            }

            return gameAchievements;
        }
        #endregion

        #region Steam Api with api key
        private ObservableCollection<AccountInfos> GetPlayerSummaries(List<ulong> steamIds)
        {
            ObservableCollection<AccountInfos> playerSummaries = null;
            if (steamIds?.Count > 0 && !CurrentAccountInfos.ApiKey.IsNullOrEmpty())
            {
                List<Models.SteamKit.SteamPlayerSummaries> steamPlayerSummaries = SteamKit.GetPlayerSummaries(CurrentAccountInfos.ApiKey, steamIds);
                playerSummaries = steamPlayerSummaries?.Select(x => new AccountInfos
                {
                    UserId = x.SteamId,
                    Avatar = x.AvatarFull,
                    Pseudo = x.PersonaName,
                    Link = x.ProfileUrl
                }).ToObservable();
            }
            return playerSummaries;
        }

        private ObservableCollection<AccountInfos> GetCurrentFriendsInfosByApi()
        {
            ObservableCollection<AccountInfos> currentFriendsInfos = null;
            if (!_currentAccountInfos.ApiKey.IsNullOrEmpty() && ulong.TryParse(_currentAccountInfos.UserId, out ulong steamId))
            {
                List<SteamFriend> friendList = SteamKit.GetFriendList(_currentAccountInfos.ApiKey, steamId);
                List<ulong> steamIds = friendList?.Select(x => x.SteamId)?.ToList() ?? new List<ulong>();
                currentFriendsInfos = GetPlayerSummaries(steamIds);

                friendList?.ForEach(x =>
                {
                    AccountInfos userInfos = currentFriendsInfos?.FirstOrDefault(y => y.UserId.IsEqual(x.SteamId.ToString()));
                    if (userInfos != null)
                    {
                        userInfos.DateAdded = x.FriendSince;
                    }
                });
            }
            return currentFriendsInfos;
        }

        private ObservableCollection<GameAchievement> GetAchievementsByApi(uint appId, AccountInfos accountInfos, ObservableCollection<GameAchievement> gameAchievements)
        {
            Logger.Info($"GetAchievementsByApi()");
            if (appId > 0 && ulong.TryParse(accountInfos.UserId, out ulong steamId) && !CurrentAccountInfos.ApiKey.IsNullOrEmpty())
            {
                List<SteamPlayerAchievement> steamPlayerAchievements = SteamKit.GetPlayerAchievements(CurrentAccountInfos.ApiKey, appId, steamId, CodeLang.GetSteamLang(Local));
                steamPlayerAchievements?.ForEach(x =>
                {
                    // Some achievements don't have a valid unlock time, use fallback date instead
                    DateTime unlockTime = x.UnlockTime.Year == 1 && x.Achieved == 1 ? new DateTime(year: 2007, month: 10, day: 10) : x.UnlockTime;
                    gameAchievements.FirstOrDefault(y => y.Id.IsEqual(x.ApiName)).DateUnlocked = unlockTime;
                });
            }
            return gameAchievements;
        }

        private ObservableCollection<AccountWishlist> GetWishlistByApi(AccountInfos accountInfos)
        {
            try
            {
                Logger.Info($"GetWishlistByApi()");
                ObservableCollection<AccountWishlist> accountWishlists = new ObservableCollection<AccountWishlist>();
                if (ulong.TryParse(accountInfos.UserId, out ulong steamId) && !CurrentAccountInfos.ApiKey.IsNullOrEmpty())
                {
                    string json = Web.DownloadStringData(string.Format(UrlWishlistApi, steamId, CurrentAccountInfos.ApiKey)).GetAwaiter().GetResult();
                    _ = Serialization.TryFromJson(json, out SteamWishlistApi steamWishlistApi, out Exception ex);
                    if (ex != null)
                    {
                        throw ex;
                    }

                    steamWishlistApi.Response.Items.ForEach(x =>
                    {
                        SteamApp steamApp = SteamApps.FirstOrDefault(y => y.AppId == x.Appid);
                        //GameInfos gameInfos = steamApp != null ? null : GetGameInfos(x.Appid.ToString(), null);

                        accountWishlists.Add(new AccountWishlist
                        {
                            Id = x.Appid.ToString(),
                            Name = steamApp?.Name ?? $"SteamApp? - {x.Appid}",
                            Link = string.Format(UrlSteamGame, x),
                            Released = null,
                            Added = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(x.DateAdded).ToUniversalTime(),
                            Image = string.Format("https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/{0}/header_292x136.jpg", x.Appid)
                        });
                    });
                }
                return accountWishlists;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }
        #endregion

        #region Steam Web        
        private bool CheckGameIsPrivateByWeb(uint appId, AccountInfos accountInfos)
        {
            Logger.Info($"CheckGameIsPrivateByWeb({appId})");
            string urlById = string.Format(UrlProfileById, accountInfos.UserId) + $"/stats/{appId}/achievements";
            string urlByPersona = string.Format(UrlProfileByName, accountInfos.Pseudo) + $"/stats/{appId}";
            List<HttpCookie> cookies = GetStoredCookies();

            string resultWeb = Web.DownloadStringData(urlById, cookies, string.Empty, true).GetAwaiter().GetResult();
            if (resultWeb.IndexOf("profile_fatalerror") == 1)
            {
                return true;
            }
            else
            {
                resultWeb = Web.DownloadStringData(urlByPersona, cookies, string.Empty, true).GetAwaiter().GetResult();
                if (resultWeb.IndexOf("profile_fatalerror") == 1)
                {
                    return true;
                }
            }
            return false;
        }

        private ObservableCollection<GameAchievement> GetAchievementsByWeb(uint appId, AccountInfos accountInfos, ObservableCollection<GameAchievement> gameAchievements)
        {
            Logger.Info($"GetAchievementsByWeb()");
            string lang = "english";
            bool needLocalized = false;
            DateTime[] unlockedDates = null;

            try
            {
                do
                {
                    string urlById = string.Format(UrlProfileById, accountInfos.UserId) + $"/stats/{appId}/achievements?l={lang}";
                    string urlByPersona = string.Format(UrlProfileByName, accountInfos.Pseudo) + $"/stats/{appId}?l={lang}";
                    needLocalized = false;

                    List<HttpCookie> cookies = GetStoredCookies();
                    string url = urlById;
                    string resultWeb = Web.DownloadStringData(url, cookies, string.Empty, true).GetAwaiter().GetResult();
                    if (resultWeb.IndexOf("achieveRow") == -1)
                    {
                        url = urlByPersona;
                        resultWeb = Web.DownloadStringData(urlByPersona, cookies, string.Empty, true).GetAwaiter().GetResult();
                    }

                    if (resultWeb.IndexOf("achieveRow") > -1)
                    {
                        IHtmlDocument htmlDocument = new HtmlParser().Parse(resultWeb);
                        int i = 0;
                        IHtmlCollection<IElement> elements = htmlDocument.QuerySelectorAll(".achieveRow");
                        foreach (IElement el in elements)
                        {
                            string UrlUnlocked = el.QuerySelector(".achieveImgHolder img")?.GetAttribute("src") ?? string.Empty;
                            string Name = el.QuerySelector(".achieveTxtHolder h3").InnerHtml;
                            string Description = el.QuerySelector(".achieveTxtHolder h5").InnerHtml;

                            DateTime DateUnlocked = default;

                            if (lang.Equals("english"))
                            {
                                bool isUnlocked = (el.GetAttribute("data-panel") ?? string.Empty).Contains("autoFocus");
                                string stringDateUnlocked = el.QuerySelector(".achieveUnlockTime")?.InnerHtml ?? string.Empty;

                                if (!stringDateUnlocked.IsNullOrEmpty())
                                {
                                    stringDateUnlocked = stringDateUnlocked.Replace("Unlocked", string.Empty).Replace("<br>", string.Empty).Trim();
                                    _ = DateTime.TryParseExact(stringDateUnlocked, new[] { "d MMM, yyyy @ h:mmtt", "d MMM @ h:mmtt", "MMM d, yyyy @ h:mmtt", "MMM d @ h:mmtt" }, new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out DateUnlocked);
                                }
                                else if (isUnlocked)
                                {
                                    DateUnlocked = i > 0 ? unlockedDates[i - 1] : DateTime.Today;
                                    Logger.Warn($"No valid date found for unlocked achievement \"{Name}\"");
                                }

                                if (unlockedDates == null)
                                {
                                    unlockedDates = new DateTime[elements.Length];
                                }
                                unlockedDates[i] = DateUnlocked;
                            }
                            else if (i < unlockedDates?.Length)
                            {
                                DateUnlocked = unlockedDates[i];
                            }

                            if (DateUnlocked != default)
                            {
                                List<GameAchievement> achievements = gameAchievements.Where(x => x.UrlUnlocked.Split('/').Last().IsEqual(UrlUnlocked.Split('/').Last())).ToList();

                                if (achievements.Count == 1)
                                {
                                    achievements[0].DateUnlocked = DateUnlocked;
                                }
                                else
                                {
                                    GameAchievement achievement = achievements.Find(x => x.Name.IsEqual(Name));
                                    if (achievement != null)
                                    {
                                        achievement.DateUnlocked = DateUnlocked;
                                    }
                                    else
                                    {
                                        if (!CodeLang.GetSteamLang(Local).IsEqual(lang))
                                        {
                                            needLocalized = true;
                                        }
                                    }
                                }
                            }
                            i++;
                        }

                        if (needLocalized)
                        {
                            lang = CodeLang.GetSteamLang(Local);
                        }
                    }
                    else if (resultWeb.IndexOf("The specified profile could not be found") > -1)
                    {

                    }
                } while (needLocalized);
            }
            catch (WebException ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return gameAchievements;
        }

        private ObservableCollection<AccountInfos> GetCurrentFriendsInfosByWeb()
        {
            try
            {
                ObservableCollection<AccountInfos> currentFriendsInfos = new ObservableCollection<AccountInfos>();
                List<HttpCookie> cookies = GetStoredCookies();
                string url = string.Format(UrlFriends, CurrentAccountInfos.UserId);
                string resultWeb = Web.DownloadStringData(url, cookies).GetAwaiter().GetResult();

                IHtmlDocument htmlDocument = new HtmlParser().Parse(resultWeb);
                IHtmlCollection<IElement> elements = htmlDocument.QuerySelectorAll(".friend_block_v2");
                foreach (IElement el in elements)
                {
                    string steamId = el.GetAttribute("data-steamid");
                    string urlProfil = el.QuerySelector("a.selectable_overlay").GetAttribute("href");
                    string avatar = el.QuerySelector("img").GetAttribute("src").Replace("_medium", "_full");
                    string pseudo = el.QuerySelector("div.friend_block_content").InnerHtml.Split(new string[] { "<br>" }, StringSplitOptions.None)[0];

                    currentFriendsInfos.Add(new AccountInfos
                    {
                        UserId = steamId,
                        Avatar = avatar,
                        Pseudo = pseudo,
                        Link = urlProfil
                    });
                }
                return currentFriendsInfos;
            }
            catch (WebException ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private ObservableCollection<AccountWishlist> GetWishlistByWeb(AccountInfos accountInfos)
        {
            try
            {
                Logger.Info($"GetWishlistByWeb()");

                ObservableCollection<AccountWishlist> accountWishlists = new ObservableCollection<AccountWishlist>();
                UserData?.RgWishlist?.ForEach(x =>
                {
                    SteamApp steamApp = SteamApps.FirstOrDefault(y => y.AppId == x);
                    accountWishlists.Add(new AccountWishlist
                    {
                        Id = x.ToString(),
                        Name = steamApp?.Name ?? $"SteamApp? - {x}",
                        Link = string.Format(UrlSteamGame, x),
                        Released = null,
                        Added = null,
                        Image = string.Format("https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/{0}/header_292x136.jpg", x.ToString())
                    });
                });

                return accountWishlists;
            }
            catch (WebException ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }
        #endregion


        private List<uint> GetDlcFromSteamDb(uint appId)
        {
            List<uint> Dlcs = new List<uint>();
            if (appId == 0)
            {
                return Dlcs;
            }

            try
            {
                WebViewSettings settings = new WebViewSettings
                {
                    UserAgent = Web.UserAgent
                };

                using (IWebView WebViewOffScreen = API.Instance.WebViews.CreateOffscreenView(settings))
                {
                    WebViewOffScreen.NavigateAndWait(string.Format(SteamDbDlc, appId));
                    string data = WebViewOffScreen.GetPageSource();

                    IHtmlDocument htmlDocument = new HtmlParser().Parse(data);
                    IHtmlCollection<IElement> SectionDlcs = htmlDocument.QuerySelectorAll("#dlc tr.app");
                    if (SectionDlcs != null)
                    {
                        foreach (IElement el in SectionDlcs)
                        {
                            string DlcIdString = el.QuerySelector("td a")?.InnerHtml;
                            if (uint.TryParse(DlcIdString, out uint DlcId))
                            {
                                Dlcs.Add(DlcId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return Dlcs;
        }

        private ObservableCollection<GameAchievement> SetExtensionsAchievementsFromSteamDb(uint appId, ObservableCollection<GameAchievement> gameAchievements)
        {
            try
            {
                Thread.Sleep(1000);
                string data = Web.DownloadStringData(string.Format(SteamDbExtensionAchievements, appId)).GetAwaiter().GetResult();
                if (Serialization.TryFromJson(data, out ExtensionsAchievements extensionsAchievementse))
                {
                    if (!extensionsAchievementse?.Success ?? true)
                    {
                        Logger.Warn($"No success in ExtensionsAchievements for {appId}");
                    }

                    int CategoryOrder = 1;
                    extensionsAchievementse?.Data?.ForEach(x =>
                    {
                        gameAchievements?.ForEach(y =>
                        {
                            if (x.AchievementApiNames.Contains(y.Id))
                            {
                                int id = (x.DlcAppId != null && x.DlcAppId != 0) ? (int)x.DlcAppId : (int)appId;
                                y.CategoryIcon = string.Format(UrlCapsuleSteam, id);
                                y.Category = x.Name.IsNullOrEmpty() ? x.DlcAppName : x.Name;
                                y.CategoryOrder = CategoryOrder;
                                y.CategoryDlc = !x.DlcAppName.IsNullOrEmpty();
                            }
                        });
                        CategoryOrder++;
                    });
                }
                else
                {
                    Logger.Warn($"No data find in ExtensionsAchievements for {appId}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }

            gameAchievements?.ForEach(y =>
            {
                if (y.CategoryOrder == 0)
                {
                    y.CategoryIcon = string.Format(UrlCapsuleSteam, appId);
                }
            });

            return gameAchievements;
        }
    }
}
