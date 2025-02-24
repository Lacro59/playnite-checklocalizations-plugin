using CommonPlayniteShared;
using CommonPlayniteShared.Common;
using CommonPluginsShared;
using CommonPluginsShared.Converters;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Models;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using static CommonPluginsShared.PlayniteTools;

namespace CommonPluginsStores
{
    public abstract class StoreApi : ObservableObject, IStoreApi
    {
        internal static ILogger Logger => LogManager.GetLogger();

        #region Account data
        protected AccountInfos _currentAccountInfos;
        public AccountInfos CurrentAccountInfos
        {
            get
            {
                _currentAccountInfos = _currentAccountInfos ?? GetCurrentAccountInfos();
                return _currentAccountInfos;
            }

            set => SetValue(ref _currentAccountInfos, value);
        }

        protected ObservableCollection<AccountInfos> _currentFriendsInfos;
        public ObservableCollection<AccountInfos> CurrentFriendsInfos
        {
            get
            {
                _currentFriendsInfos = _currentFriendsInfos ?? GetCurrentFriendsInfos();
                return _currentFriendsInfos;
            }

            set => SetValue(ref _currentFriendsInfos, value);
        }

        protected ObservableCollection<AccountGameInfos> _currentGamesInfos;
        public ObservableCollection<AccountGameInfos> CurrentGamesInfos
        {
            get
            {
                _currentGamesInfos = _currentGamesInfos ?? GetAccountGamesInfos(CurrentAccountInfos);
                return _currentGamesInfos;
            }

            set => SetValue(ref _currentGamesInfos, value);
        }

        public ObservableCollection<GameDlcOwned> CurrentGamesDlcsOwned
        {
            get
            {
                ObservableCollection<GameDlcOwned> currentGamesDlcsOwned = LoadGamesDlcsOwned() ?? GetGamesDlcsOwned() ?? LoadGamesDlcsOwned(false);
                _ = SaveGamesDlcsOwned(currentGamesDlcsOwned);
                return currentGamesDlcsOwned;
            }
        }
        #endregion

        public StoreSettings StoreSettings { get; set; } = new StoreSettings();

        protected bool? isUserLoggedIn;
        public bool IsUserLoggedIn
        {
            get
            {
                isUserLoggedIn = isUserLoggedIn ?? GetIsUserLoggedIn();
                if ((bool)isUserLoggedIn)
                {
                    _ = SetStoredCookies(GetWebCookies());
                }
                return (bool)isUserLoggedIn;
            }

            set => SetValue(ref isUserLoggedIn, value);
        }

        internal string PluginName { get; }
        internal string ClientName { get; }
        internal string ClientNameLog { get; }
        internal string Local { get; set; } = "en_US";

        internal string PathStoresData { get; }
        internal string FileUser { get; }
        internal string FileCookies { get; }
        internal string FileGamesDlcsOwned { get; }

        internal List<string> CookiesDomains { get; set; }

        internal StoreToken AuthToken { get; set; }
        internal ExternalPlugin PluginLibrary { get; }


        public StoreApi(string pluginName, ExternalPlugin pluginLibrary, string clientName)
        {
            PluginName = pluginName;
            PluginLibrary = pluginLibrary;
            ClientName = clientName;
            ClientNameLog = clientName.RemoveWhiteSpace();

            PathStoresData = Path.Combine(PlaynitePaths.ExtensionsDataPath, "StoresData"); 
            FileUser = Path.Combine(PathStoresData, CommonPlayniteShared.Common.Paths.GetSafePathName($"{ClientNameLog}_User.dat"));
            FileCookies = Path.Combine(PathStoresData, CommonPlayniteShared.Common.Paths.GetSafePathName($"{ClientNameLog}_Cookies.dat"));
            FileGamesDlcsOwned = Path.Combine(PathStoresData, CommonPlayniteShared.Common.Paths.GetSafePathName($"{ClientNameLog}_GamesDlcsOwned.json"));

            FileSystem.CreateDirectory(PathStoresData);
        }


        #region Cookies
        /// <summary>
        /// Read the last identified cookies stored.
        /// </summary>
        /// <returns></returns>
        internal List<HttpCookie> GetStoredCookies()
        {
            string InfoMessage = "No stored cookies";

            if (File.Exists(FileCookies))
            {
                try
                {
                    List<HttpCookie> StoredCookies = Serialization.FromJson<List<HttpCookie>>(
                        Encryption.DecryptFromFile(
                            FileCookies,
                            Encoding.UTF8,
                            WindowsIdentity.GetCurrent().User.Value));

                    List<HttpCookie> findExpired = StoredCookies.FindAll(x => x.Expires != null && (DateTime)x.Expires <= DateTime.Now);

                    FileInfo fileInfo = new FileInfo(FileCookies);
                    bool isExpired = fileInfo.LastWriteTime.AddDays(1) < DateTime.Now;

                    if (findExpired?.Count > 0 || isExpired)
                    {
                        InfoMessage = "Expired cookies";
                    }
                    else
                    {
                        return StoredCookies;
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, "Failed to load saved cookies");
                }
            }

            Logger.Warn(InfoMessage);
            List<HttpCookie> httpCookies = GetWebCookies();
            if (httpCookies?.Count > 0)
            {
                _ = SetStoredCookies(httpCookies);
                return httpCookies;
            }

            return null;
        }

        /// <summary>
        /// Save the last identified cookies stored.
        /// </summary>
        /// <param name="httpCookies"></param>
        internal bool SetStoredCookies(List<HttpCookie> httpCookies)
        {
            try
            {
                if (httpCookies?.Count() > 0)
                {
                    FileSystem.CreateDirectory(Path.GetDirectoryName(FileCookies));
                    Encryption.EncryptToFile(
                        FileCookies,
                        Serialization.ToJson(httpCookies),
                        Encoding.UTF8,
                        WindowsIdentity.GetCurrent().User.Value);
                    return true;
                }
                else
                {
                    Logger.Warn($"No cookies saved for {PluginName}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, "Failed to save cookies");
            }

            return false;
        }

        /// <summary>
        /// Get cookies in WebView or another method.
        /// </summary>
        /// <returns></returns>
        internal virtual List<HttpCookie> GetWebCookies()
        {
            try
            {
                using (IWebView webView = API.Instance.WebViews.CreateOffscreenView())
                {
                    List<HttpCookie> httpCookies = CookiesDomains?.Count > 0
                        ? webView.GetCookies()?.Where(x => CookiesDomains.Any(y => y.Contains(x?.Domain, StringComparison.InvariantCultureIgnoreCase)))?.ToList() ?? new List<HttpCookie>()
                        : webView.GetCookies()?.Where(x => x?.Domain?.Contains(ClientName, StringComparison.InvariantCultureIgnoreCase) ?? false)?.ToList() ?? new List<HttpCookie>();
                    return httpCookies;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
                return new List<HttpCookie>();
            }
        }

        internal virtual List<HttpCookie> GetNewWebCookies(string url)
        {
            try
            {
                using (IWebView webView = API.Instance.WebViews.CreateOffscreenView())
                {
                    List<HttpCookie> oldCookies = GetStoredCookies();
                    oldCookies?.ForEach(x =>
                    {
                        webView.SetCookies(url, x);
                    });

                    webView.NavigateAndWait(url);

                    List<HttpCookie> httpCookies = CookiesDomains?.Count > 0
                        ? webView.GetCookies()?.Where(x => CookiesDomains.Any(y => y.Contains(x?.Domain, StringComparison.InvariantCultureIgnoreCase)))?.ToList() ?? new List<HttpCookie>()
                        : webView.GetCookies()?.Where(x => x?.Domain?.Contains(ClientName, StringComparison.InvariantCultureIgnoreCase) ?? false)?.ToList() ?? new List<HttpCookie>();
                    return httpCookies;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
                return new List<HttpCookie>();
            }
        }
        #endregion


        #region Configuration
        public void ResetIsUserLoggedIn()
        {
            isUserLoggedIn = null;
        }

        protected abstract bool GetIsUserLoggedIn();

        /// <summary>
        /// Set data language.
        /// </summary>
        /// <param name="local">ISO 15897</param>
        public void SetLanguage(string local)
        {
            Local = local;
        }

        public void SetForceAuth(bool forceAuth)
        {
            StoreSettings.ForceAuth = forceAuth;
        }

        public virtual void Login()
        {
            throw new NotImplementedException();
        }

        public AccountInfos LoadCurrentUser()
        {
            if (FileSystem.FileExists(FileUser))
            {
                try
                {
                    string user = Encryption.DecryptFromFile(
                        FileUser,
                        Encoding.UTF8,
                        WindowsIdentity.GetCurrent().User.Value);

                    _ = Serialization.TryFromJson(user, out AccountInfos accountInfos);
                    return accountInfos;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName, $"Failed to load {ClientName} user.");
                }
            }

            return null;
        }

        public virtual void SaveCurrentUser()
        {
            if (CurrentAccountInfos != null)
            {
                FileSystem.PrepareSaveFile(FileUser);
                Encryption.EncryptToFile(
                    FileUser,
                    Serialization.ToJson(CurrentAccountInfos),
                    Encoding.UTF8,
                    WindowsIdentity.GetCurrent().User.Value);
            }
        }
        #endregion


        #region Current user
        /// <summary>
        /// Get current user info.
        /// </summary>
        /// <returns></returns>
        protected abstract AccountInfos GetCurrentAccountInfos();

        /// <summary>
        /// Get current user's friends info.
        /// </summary>
        /// <returns></returns>
        protected virtual ObservableCollection<AccountInfos> GetCurrentFriendsInfos()
        {
            return null;
        }

        /// <summary>
        /// Get all game's owned for current user.
        /// </summary>
        /// <returns></returns>
        protected virtual ObservableCollection<GameDlcOwned> GetGamesOwned()
        {
            return null;
        }
        #endregion


        #region User details
        /// <summary>
        /// Get the user's games list.
        /// /// </summary>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public abstract ObservableCollection<AccountGameInfos> GetAccountGamesInfos(AccountInfos accountInfos);

        /// <summary>
        /// Get a list of a game's achievements with a user's possessions.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public virtual ObservableCollection<GameAchievement> GetAchievements(string id, AccountInfos accountInfos)
        {
            return null;
        }

        /// <summary>
        /// Get achievements SourceLink.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public virtual SourceLink GetAchievementsSourceLink(string name, string id, AccountInfos accountInfos)
        {
            return null;
        }

        public virtual ObservableCollection<AccountWishlist> GetWishlist(AccountInfos accountInfos)
        {
            return null;
        }

        public virtual bool RemoveWishlist(string id)
        {
            return false;
        }
        #endregion


        #region Game
        /// <summary>
        /// Get game informations.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public virtual GameInfos GetGameInfos(string id, AccountInfos accountInfos)
        {
            return null;
        }

        /// <summary>
        /// Get dlc informations for a game.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public virtual ObservableCollection<DlcInfos> GetDlcInfos(string id, AccountInfos accountInfos)
        {
            return null;
        }
        #endregion


        #region Games owned
        private ObservableCollection<GameDlcOwned> LoadGamesDlcsOwned(bool onlyNow = true)
        {
            if (File.Exists(FileGamesDlcsOwned))
            {
                try
                {
                    DateTime dateLastWrite = File.GetLastWriteTime(FileGamesDlcsOwned);
                    if (onlyNow && dateLastWrite.AddMinutes(5) <= DateTime.Now)
                    {
                        return null;
                    }

                    if (!onlyNow)
                    {
                        ShowNotificationOldData(dateLastWrite);
                    }

                    return Serialization.FromJsonFile<ObservableCollection<GameDlcOwned>>(FileGamesDlcsOwned);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return null;
        }

        private bool SaveGamesDlcsOwned(ObservableCollection<GameDlcOwned> gamesDlcsOwned)
        {
            try
            {
                FileSystem.PrepareSaveFile(FileGamesDlcsOwned);
                File.WriteAllText(FileGamesDlcsOwned, Serialization.ToJson(gamesDlcsOwned));
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return false;
            }
        }


        internal virtual ObservableCollection<GameDlcOwned> GetGamesDlcsOwned()
        {
            return null;
        }


        internal virtual bool IsDlcOwned(string id)
        {
            try
            {
                bool IsOwned = CurrentGamesDlcsOwned?.Count(x => x.Id.IsEqual(id)) > 0;
                return IsOwned;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return false;
            }
        }
        #endregion


        internal void ShowNotificationOldData(DateTime dateLastWrite)
        {
            LocalDateTimeConverter localDateTimeConverter = new LocalDateTimeConverter();
            string formatedDateLastWrite = localDateTimeConverter.Convert(dateLastWrite, null, null, CultureInfo.CurrentCulture).ToString();
            Logger.Warn($"Use saved UserData - {formatedDateLastWrite}");
            API.Instance.Notifications.Add(new NotificationMessage(
                $"{PluginName}-{ClientNameLog}-LoadGamesDlcsOwned",
                $"{PluginName}" + Environment.NewLine
                    + string.Format(ResourceProvider.GetString("LOCCommonNotificationOldData"), ClientName, formatedDateLastWrite),
                NotificationType.Info,
                () =>
                {
                    ResetIsUserLoggedIn();
                    ShowPluginSettings(PluginLibrary);
                }
            ));
        }


        public static float CalcGamerScore(float value)
        {
            float gamerScore = 15;
            if (value < 2)
            {
                gamerScore = 180;
            }
            else if (value < 10)
            {
                gamerScore = 90;
            }
            else if (value < 30)
            {
                gamerScore = 30;
            }
            return gamerScore;
        }
    }


    public class StoreSettings
    {
        public bool UseApi { get; set; } = false;


        private bool _useAuth;
        public bool UseAuth
        {
            get => ForceAuth || _useAuth;
            set => _useAuth = value;
        }

        public bool ForceAuth { get; set; } = false;
    }
}
