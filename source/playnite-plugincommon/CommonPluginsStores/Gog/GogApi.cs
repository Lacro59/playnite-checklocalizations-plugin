using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using CommonPlayniteShared.Common;
using CommonPluginsShared;
using CommonPluginsShared.Converters;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Models;
using CommonPluginsStores.Gog.Models;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static CommonPluginsShared.PlayniteTools;

namespace CommonPluginsStores.Gog
{
    // https://gogapidocs.readthedocs.io/en/latest/
    public class GogApi : StoreApi
    {
        #region Url
        private static string UrlAccountInfo => @"https://menu.gog.com/v1/account/basic";

        private static string UrlBase => @"https://www.gog.com";

        private static string UrlLogin => UrlBase + @"/account/";

        private static string UrlUserData => UrlBase + @"/userData.json";

        private static string UrlUser => UrlBase + @"/u/{0}";
        private static string UrlUserFriends => UrlUser + @"/friends";
        private static string UrlUserGames => UrlUser + @"/games/stats?page={1}";
        private static string UrlUserGameAchievements => UrlUser + @"/game/{1}?sort=user_unlock_date&sort_user_id={2}";
        private static string UrlWishlist => UrlUser + @"/wishlist";

        private static string UrlGogLang => UrlBase + @"/user/changeLanguage/{0}";
        private static string UrlGogGame => UrlBase + @"/game/{0}";
        #endregion

        #region Url API
        private static string UrlApiGamePlay => @"https://gameplay.gog.com";
        private static string UrlApi => @"https://api.gog.com";
        private static string UrlEmbed => @"https://embed.gog.com";

        private static string UrlApiGamePlayUserAchievements => UrlApiGamePlay + @"/clients/{0}/users/{1}/achievements";
        private static string UrlApiGamePlayFriendAchievements => UrlApiGamePlay + @"/clients/{0}/users/{1}/friends_achievements_unlock_progresses";

        private static string UrlApiGameInfo => UrlApi + @"/products/{0}?expand=description&locale={1}";

        private static string UrlApiPrice => UrlApi + @"/products/prices?ids={0}&countryCode={1}&currency={2}";

        private static string UrlApiWishlist => UrlEmbed + @"/user/wishlist.json";
        private static string UrlApiRemoveWishlist => UrlEmbed + @"/user/wishlist/remove/{0}";

        private static string UrlFriends => UrlEmbed + @"/users/info/{0}?expand=friendStatus";
        private static string UrlUserOwned => UrlEmbed + @"/user/data/games";
        #endregion

        private string FileUserDataOwned { get; }

        private UserDataOwned UserDataOwned => LoadUserDataOwned() ?? GetUserDataOwnedData() ?? LoadUserDataOwned(false);

        private AccountBasicResponse _accountBasic;
        private AccountBasicResponse AccountBasic
        {
            get
            {
                if (!_accountBasic?.IsLoggedIn ?? true)
                {
                    _accountBasic = GetAccountInfo();
                }
                return _accountBasic;
            }

            set => _accountBasic = value;
        }


        private static StoreCurrency LocalCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };


        public GogApi(string pluginName, ExternalPlugin pluginLibrary) : base(pluginName, pluginLibrary, "GOG")
        {
            FileUserDataOwned = Path.Combine(PathStoresData, "GOG_UserDataOwned.json");
        }

        #region Configuration
        protected override bool GetIsUserLoggedIn()
        {
            if (!CurrentAccountInfos.IsPrivate && !StoreSettings.UseAuth)
            {
                return !CurrentAccountInfos.UserId.IsNullOrEmpty();
            }

            bool isLogged = CheckIsUserLoggedIn();
            if (isLogged)
            {
                string response = Web.DownloadStringData(UrlAccountInfo, GetStoredCookies()).GetAwaiter().GetResult();
                _ = Serialization.TryFromJson(response, out AccountBasicResponse accountBasicResponse);
                AuthToken = new StoreToken
                {
                    Token = accountBasicResponse.AccessToken
                };

                _ = SetStoredCookies(GetNewWebCookies(UrlLogin));
            }
            else
            {
                AuthToken = null;
            }

            return isLogged;
        }

        public override void Login()
        {
            try
            {
                ResetIsUserLoggedIn();
                GogLogin();

                if (AccountBasic?.IsLoggedIn ?? false)
                {
                    CurrentAccountInfos = new AccountInfos
                    {
                        UserId = AccountBasic.UserId,
                        Pseudo = AccountBasic.Username,
                        Link = string.Format(UrlUser, AccountBasic.Username),
                        Avatar = AccountBasic.Avatars.MenuUserAvBig2,
                        IsPrivate = true,
                        IsCurrent = true
                    };
                    SaveCurrentUser();
                    _ = GetCurrentAccountInfos();

                    Logger.Info($"{PluginName} logged");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }
        }

        /// <summary>
        /// Set currency.
        /// </summary>
        /// <param name="currency"></param>
        public void SetCurrency(StoreCurrency currency)
        {
            LocalCurrency = currency;
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

                    string reponse = Web.DownloadStringData(string.Format(UrlUser, accountInfos.Pseudo), GetStoredCookies()).GetAwaiter().GetResult();
                    string jsonDataString = Tools.GetJsonInString(reponse, @"window.profilesData.currentUser[ ]?=[ ]?");
                    _ = Serialization.TryFromJson(jsonDataString, out ProfileUser profileUser);

                    CurrentAccountInfos.Avatar = profileUser?.Avatar ?? CurrentAccountInfos.Avatar;
                    CurrentAccountInfos.IsPrivate = !CheckIsPublic(accountInfos).GetAwaiter().GetResult();
                    CurrentAccountInfos.AccountStatus = CurrentAccountInfos.IsPrivate ? AccountStatus.Private : AccountStatus.Public;
                });
                return accountInfos;
            }
            return new AccountInfos { IsCurrent = true };
        }

        protected override ObservableCollection<AccountInfos> GetCurrentFriendsInfos()
        {
            if (!IsUserLoggedIn)
            {
                return null;
            }

            try
            {
                string reponse = Web.DownloadStringData(string.Format(UrlUserFriends, CurrentAccountInfos.Pseudo), GetStoredCookies()).GetAwaiter().GetResult();
                if (reponse.IndexOf("window.profilesData.currentUser") == -1)
                {
                    IsUserLoggedIn = false;
                    Logger.Warn($"Not found: window.profilesData.profileUserFriends");
                    return null;
                }

                string jsonDataString = Tools.GetJsonInString(reponse, "window.profilesData.profileUserFriends[ ]?=[ ]?");
                _ = Serialization.TryFromJson(jsonDataString, out List<ProfileUserFriends> profileUserFriends);

                if (profileUserFriends == null)
                {
                    return null;
                }

                ObservableCollection<AccountInfos> accountsInfos = new ObservableCollection<AccountInfos>();
                profileUserFriends.ForEach(x =>
                {
                    _ = DateTime.TryParse(x.DateAccepted.Date, out DateTime DateAdded);
                    string userId = x.User.Id;
                    string avatar = x.User.Avatar.Replace("\\", string.Empty);
                    string pseudo = x.User.Username;
                    string link = string.Format(UrlUserFriends, pseudo);

                    AccountInfos userInfos = new AccountInfos
                    {
                        DateAdded = DateAdded,
                        UserId = userId,
                        Avatar = avatar,
                        Pseudo = pseudo,
                        Link = link
                    };
                    accountsInfos.Add(userInfos);
                });

                return accountsInfos;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }
        #endregion

        #region User details
        public override ObservableCollection<AccountGameInfos> GetAccountGamesInfos(AccountInfos accountInfos)
        {
            if (!IsUserLoggedIn)
            {
                return null;
            }

            try
            {
                ObservableCollection<AccountGameInfos> accountGamesInfos = new ObservableCollection<AccountGameInfos>();
                for (int idx = 1; idx < 50; idx++)
                {
                    try
                    {
                        string response = Web.DownloadStringData(string.Format(UrlUserGames, accountInfos.Pseudo, idx), GetStoredCookies()).GetAwaiter().GetResult();
                        _ = Serialization.TryFromJson(response, out ProfileGames profileGames);

                        if (profileGames == null)
                        {
                            break;
                        }

                        profileGames?.Embedded?.Items?.ForEach(x =>
                        {
                            string id = x.Game.Id;
                            string name = x.Game.Title;

                            bool isCommun = false;
                            if (!accountInfos.IsCurrent)
                            {
                                isCommun = CurrentGamesInfos?.Where(y => y.Id.IsEqual(id))?.Count() != 0;
                            }

                            long playtime = 0;
                            foreach (dynamic data in (dynamic)x.Stats)
                            {
                                long.TryParse(((dynamic)x.Stats)[data.Path]["playtime"].ToString(), out playtime);
                                playtime *= 60;

                                if (playtime != 0)
                                {
                                    break;
                                }
                            }

                            string link = UrlBase + x.Game.Url.Replace("\\", string.Empty);
                            ObservableCollection<GameAchievement> Achievements = GetAchievements(id, accountInfos);

                            AccountGameInfos accountGameInfos = new AccountGameInfos
                            {
                                Id = id,
                                Name = name,
                                Link = link,
                                IsCommun = isCommun,
                                Achievements = Achievements,
                                Playtime = playtime
                            };
                            accountGamesInfos.Add(accountGameInfos);
                        });
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, true, PluginName);
                    }
                }

                return accountGamesInfos;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }

        public override ObservableCollection<GameAchievement> GetAchievements(string id, AccountInfos accountInfos)
        {
            return accountInfos.IsPrivate || StoreSettings.UseAuth ? GetAchievementsPrivate(id, accountInfos, false) : GetAchievementsPublic(id, accountInfos);
        }

        private ObservableCollection<GameAchievement> GetAchievementsPrivate(string id, AccountInfos accountInfos, bool isRetry)
        {
            if (!IsUserLoggedIn)
            {
                return null;
            }

            try
            {
                string url = string.Empty;
                if (accountInfos.IsCurrent)
                {
                    url = string.Format(UrlApiGamePlayUserAchievements, id, accountInfos.UserId);
                }
                else
                {
                    // TODO Works ?
                    url = string.Format(UrlApiGamePlayFriendAchievements, id, accountInfos.UserId);
                }

                string urlLang = string.Format(UrlGogLang, CodeLang.GetGogLang(Local).ToLower());
                string reponse = Web.DownloadStringData(url, AuthToken?.Token, urlLang).GetAwaiter().GetResult();

                ObservableCollection<GameAchievement> gameAchievements = new ObservableCollection<GameAchievement>();
                if (!reponse.IsNullOrEmpty() && Serialization.TryFromJson(reponse, out Achievements achievements) && achievements?.TotalCount > 0)
                {
                    achievements.Items.ForEach(x =>
                    {
                        GameAchievement gameAchievement = new GameAchievement
                        {
                            Id = x.AchievementKey,
                            Name = x.Name,
                            Description = x.Description,
                            UrlUnlocked = x.ImageUrlUnlocked,
                            UrlLocked = x.ImageUrlLocked,
                            DateUnlocked = x.DateUnlocked == null ? default : (DateTime)x.DateUnlocked,
                            Percent = (float)x.Rarity,
                            GamerScore = CalcGamerScore((float)x.Rarity),
                            IsHidden = !x.Visible
                        };
                        gameAchievements.Add(gameAchievement);
                    });
                }

                return gameAchievements;
            }
            catch (Exception ex)
            {
                // Reset login status when 401 error
                if (ex.Message.Contains("401") || ex.Message.Contains("access_denied"))
                {
                    if (isRetry)
                    {
                        Logger.Warn($"Error 401");
                        ResetIsUserLoggedIn();
                    }
                    else
                    {
                        Logger.Warn($"Error 401 - Wait and retry");
                        Thread.Sleep(5000);
                        return GetAchievementsPrivate(id, accountInfos, true);
                    }
                }
                else
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return null;
        }

        private ObservableCollection<GameAchievement> GetAchievementsPublic(string id, AccountInfos accountInfos)
        {
            try
            {
                string url = string.Format(UrlUserGameAchievements, accountInfos.Pseudo, id, accountInfos.UserId);
                string urlLang = string.Format(UrlGogLang, CodeLang.GetGogLang(Local).ToLower());
                string response = Web.DownloadStringDataWithUrlBefore(url, urlLang).GetAwaiter().GetResult();
                string jsonDataString = Tools.GetJsonInString(response, "(?<=window.profilesData.achievements\\s=\\s)");

                ObservableCollection<GameAchievement> gameAchievements = new ObservableCollection<GameAchievement>();
                if (Serialization.TryFromJson(jsonDataString, out dynamic data))
                {
                    foreach(dynamic ach in data)
                    {
                        dynamic item = ach["achievement"];
                        AchItem achItem = Serialization.FromJson<AchItem>(item.ToString());
                        dynamic stats = ach["stats"];

                        GameAchievement gameAchievement = new GameAchievement
                        {
                            Id = achItem.Id,
                            Name = achItem.Name,
                            Description = achItem.Description,
                            UrlUnlocked = achItem.ImageUrlUnlocked2,
                            UrlLocked = achItem.ImageUrlLocked2,
                            DateUnlocked = !(bool)stats[accountInfos.UserId]["isUnlocked"] ? default : new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((int)stats[accountInfos.UserId]["unlockDate"]).ToUniversalTime(),
                            Percent = (float)achItem.Rarity,
                            GamerScore = CalcGamerScore((float)achItem.Rarity),
                            IsHidden = !achItem.Visible
                        };
                        gameAchievements.Add(gameAchievement);
                    }
                }

                return gameAchievements;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error remove {id} in {ClientName} wishlist", true, PluginName);
            }

            return null;
        }

        public override SourceLink GetAchievementsSourceLink(string name, string id, AccountInfos accountInfos)
        {
            return new SourceLink
            {
                GameName = name,
                Name = ClientName,
                Url = string.Format(UrlUserGameAchievements, accountInfos.Pseudo, id, accountInfos.UserId)
            };
        }

        public override ObservableCollection<AccountWishlist> GetWishlist(AccountInfos accountInfos)
        {
            if (accountInfos != null)
            {
                string response;
                bool HasError = false;
                ObservableCollection<AccountWishlist> data = new ObservableCollection<AccountWishlist>();

                // With api
                if (accountInfos.IsCurrent)
                {
                    try
                    {
                        using (IWebView WebViewOffscreen = API.Instance.WebViews.CreateOffscreenView())
                        {
                            WebViewOffscreen.NavigateAndWait(UrlApiWishlist);
                            response = WebViewOffscreen.GetPageText();

                            if (Serialization.TryFromJson(response, out WishlistResult wishlistResult))
                            {
                                foreach (dynamic gameWishlist in wishlistResult.Wishlist)
                                {
                                    if ((bool)gameWishlist.Value)
                                    {
                                        string StoreId = gameWishlist.Name;
                                        GameInfos gameInfos = GetGameInfos(StoreId, null);
                                        if (gameInfos != null)
                                        {
                                            data.Add(new AccountWishlist
                                            {
                                                Id = gameInfos.Id,
                                                Name = gameInfos.Name,
                                                Link = gameInfos.Link,
                                                Released = gameInfos.Released,
                                                Added = null,
                                                Image = gameInfos.Image
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Logger.Warn($"GOG is disconnected");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, true, PluginName);
                        HasError = true;
                    }
                }

                if (!HasError && data.Count > 0)
                {
                    return data;
                }

                // Without Api
                try
                {
                    using (IWebView WebViewOffscreen = API.Instance.WebViews.CreateOffscreenView())
                    {
                        WebViewOffscreen.NavigateAndWait(string.Format(UrlWishlist, accountInfos.Pseudo));
                        response = WebViewOffscreen.GetPageSource();

                        // Get game information for wishlist
                        if (!response.IsNullOrEmpty())
                        {
                            HtmlParser parser = new HtmlParser();
                            IHtmlDocument HtmlRequirement = parser.Parse(response);

                            foreach (IElement el in HtmlRequirement.QuerySelectorAll(".product-row-wrapper .product-state-holder"))
                            {
                                string StoreId = el.GetAttribute("gog-product");
                                GameInfos gameInfos = GetGameInfos(StoreId, null);
                                if (gameInfos != null)
                                {
                                    data.Add(new AccountWishlist
                                    {
                                        Id = gameInfos.Id,
                                        Name = gameInfos.Name,
                                        Link = gameInfos.Link,
                                        Released = gameInfos.Released,
                                        Added = null,
                                        Image = gameInfos.Image
                                    });
                                }
                            }
                        }

                        return data;
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error in {ClientName} wishlist", true, PluginName);
                }
            }

            return null;
        }

        public override bool RemoveWishlist(string id)
        {
            if (IsUserLoggedIn)
            {
                try
                {
                    using (IWebView WebViewOffscreen = API.Instance.WebViews.CreateOffscreenView())
                    {
                        WebViewOffscreen.NavigateAndWait(string.Format(UrlApiRemoveWishlist, id));
                        return WebViewOffscreen.GetPageSource().ToLower().IndexOf("unable to remove product from wishlist") == -1;
                    }
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
        public override GameInfos GetGameInfos(string id, AccountInfos accountInfos)
        {
            try
            {
                string url = string.Format(UrlApiGameInfo, id, CodeLang.GetGogLang(Local).ToLower());
                string reponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
                _ = Serialization.TryFromJson(reponse, out Models.ProductApiDetail productApiDetail);

                GameInfos gameInfos = new GameInfos
                {
                    Id = productApiDetail?.Id.ToString(),
                    Name = productApiDetail?.Title,
                    Link = productApiDetail?.ProductLinks?.ProductCard,
                    Image = "https:" + productApiDetail?.ProductImages?.Logo2x,
                    Description = RemoveDescriptionPromos(productApiDetail.ProductDescription.Full).Trim(),
                    Released = productApiDetail?.ReleaseDate
                };

                // DLC
                string stringDlcs = Serialization.ToJson(productApiDetail?.Dlcs);
                if (!stringDlcs.IsNullOrEmpty() && !stringDlcs.IsEqual("[]"))
                {
                    GogDlcs DlcsData = Serialization.FromJson<GogDlcs>(stringDlcs);
                    ObservableCollection<DlcInfos> Dlcs = GetDlcInfos(DlcsData, accountInfos);
                    gameInfos.Dlcs = Dlcs;
                }

                return gameInfos;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }

        public override ObservableCollection<DlcInfos> GetDlcInfos(string id, AccountInfos accountInfos)
        {
            string url = string.Format(UrlApiGameInfo, id, CodeLang.GetGogLang(Local).ToLower());
            string reponse = Web.DownloadStringData(url).GetAwaiter().GetResult();
            _ = Serialization.TryFromJson(reponse, out ProductApiDetail productApiDetail, out Exception ex);
            if (ex != null)
            {
                ManageException($"No game data for {id}", ex, reponse.Contains("404"));
            }

            string stringDlcs = Serialization.ToJson(productApiDetail?.Dlcs);
            if (!stringDlcs.IsNullOrEmpty() && !stringDlcs.IsEqual("[]"))
            {
                GogDlcs dlcsData = Serialization.FromJson<GogDlcs>(stringDlcs);
                return GetDlcInfos(dlcsData, accountInfos);
            }

            return null;
        }

        private ObservableCollection<DlcInfos> GetDlcInfos(GogDlcs dlcsData, AccountInfos accountInfos)
        {
            ObservableCollection<DlcInfos> Dlcs = new ObservableCollection<DlcInfos>();

            if (dlcsData?.Products == null)
            {
                return Dlcs;
            }

            foreach (Product el in dlcsData?.Products)
            {
                try
                {
                    string dataDlc = Web.DownloadStringData(string.Format(UrlApiGameInfo, el.Id, CodeLang.GetGogLang(Local).ToLower())).GetAwaiter().GetResult();
                    if (!dataDlc.Contains("<!DOCTYPE html>", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = Serialization.TryFromJson(dataDlc, out ProductApiDetail productApiDetailDlc, out Exception ex);
                        if (ex != null)
                        {
                            ManageException($"No Dlc data for {el.Id}", ex, dataDlc.Contains("404"));
                        }

                        bool IsOwned = false;
                        if (accountInfos != null && accountInfos.IsCurrent)
                        {
                            IsOwned = IsDlcOwned(el.Id.ToString());
                        }

                        DlcInfos dlc = new DlcInfos
                        {
                            Id = el.Id.ToString(),
                            Name = productApiDetailDlc?.Title,
                            Description = RemoveDescriptionPromos(productApiDetailDlc?.ProductDescription?.Full).Trim(),
                            Image = "https:" + productApiDetailDlc?.ProductImages?.Logo2x,
                            Link = string.Format(UrlGogGame, productApiDetailDlc?.Slug),
                            IsOwned = IsOwned
                        };

                        Dlcs.Add(dlc);
                    }
                    else
                    {
                        Logger.Warn($"No dlc data for {el.Id}");
                    }
                }
                catch (Exception ex)
                {
                    ManageException(el.Id.ToString(), ex, false);
                }
            }

            // Price
            if (Dlcs?.Count > 0)
            {
                try
                {
                    PriceData priceData = GetPrice(Dlcs.Select(x => x.Id).ToList(), Local, LocalCurrency);
                    string dataObjString = Serialization.ToJson(priceData?.DataObj["_embedded"]);
                    _ = Serialization.TryFromJson(dataObjString, out PriceResult priceResult, out Exception ex);
                    if (ex != null)
                    {
                        ManageException($"No price data for {string.Join(",", Dlcs.Select(x => x.Id))}", ex, dataObjString.Contains("404"));
                    }

                    priceResult?.Items?.ForEach(y =>
                    {
                        int idx = Dlcs.ToList().FindIndex(x => x.Id.IsEqual(y.Embedded.Product.Id.ToString()));
                        if (idx > -1)
                        {
                            _ = double.TryParse(y.Embedded.Prices[0].FinalPrice.Replace(priceData.CodeCurrency, string.Empty, StringComparison.InvariantCultureIgnoreCase), out double price);
                            _ = double.TryParse(y.Embedded.Prices[0].BasePrice.Replace(priceData.CodeCurrency, string.Empty, StringComparison.InvariantCultureIgnoreCase), out double priceBase);

                            price *= 0.01;
                            priceBase *= 0.01;

                            Dlcs[idx].Price = price + " " + priceData.SymbolCurrency;
                            Dlcs[idx].PriceBase = priceBase + " " + priceData.SymbolCurrency;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return Dlcs;
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
                UserDataOwned?.Owned?.ForEach(x =>
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

        #region GOG
        public async Task<bool> CheckIsPublic(AccountInfos accountInfos)
        {
            try
            {
                string url = string.Format(UrlUser, accountInfos.Pseudo);
                string response = await Web.DownloadStringData(url);
                return !response.Contains("hook-test=\"isPrivate\"");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return false;
        }

        private bool CheckIsUserLoggedIn()
        {
            return !AccountBasic?.AccessToken.IsNullOrEmpty() ?? false;
        }

        private void GogLogin()
        {
            using (IWebView webView = API.Instance.WebViews.CreateView(new WebViewSettings
            {
                WindowWidth = 580,
                WindowHeight = 700,
                // This is needed otherwise captcha won't pass
                UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 Vivaldi/5.5.2805.50"
            }))
            {
                webView.LoadingChanged += async (s, e) =>
                {
                    string url = webView.GetCurrentAddress();
                    if (!url.EndsWith("#openlogin"))
                    {
                        bool loggedIn = await Task.Run(() =>
                        {
                            using (IWebView webViewBackground = API.Instance.WebViews.CreateOffscreenView())
                            {
                                webViewBackground.NavigateAndWait(UrlAccountInfo);
                                string stringInfo = webViewBackground.GetPageText();
                                _ = Serialization.TryFromJson(stringInfo, out AccountBasicResponse accountBasicResponse);
                                AccountBasic = accountBasicResponse;
                                return AccountBasic?.IsLoggedIn ?? false;
                            }
                        });
                        if (loggedIn)
                        {
                            _ = SetStoredCookies(GetWebCookies());
                            webView.Close();
                        }
                    }
                };

                webView.DeleteDomainCookies(".gog.com");
                webView.Navigate(UrlLogin);
                _ = webView.OpenDialog();
            }
        }

        private AccountBasicResponse GetAccountInfo()
        {
            string response = Web.DownloadStringData(UrlAccountInfo, GetStoredCookies()).GetAwaiter().GetResult();
            _ = Serialization.TryFromJson(response, out AccountBasicResponse accountBasicResponse);
            return accountBasicResponse;
        }


        private UserDataOwned LoadUserDataOwned(bool onlyNow = true)
        {
            if (File.Exists(FileUserDataOwned))
            {
                try
                {
                    DateTime dateLastWrite = File.GetLastWriteTime(FileUserDataOwned);
                    if (onlyNow && dateLastWrite.AddMinutes(5) <= DateTime.Now)
                    {
                        return null;
                    }

                    if (!onlyNow)
                    {
                        ShowNotificationOldData(dateLastWrite);
                    }

                    return Serialization.FromJsonFile<UserDataOwned>(FileUserDataOwned);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return null;
        }

        private UserDataOwned GetUserDataOwnedData()
        {
            try
            {
                string data = Web.DownloadStringData(UrlUserOwned, AuthToken.Token).GetAwaiter().GetResult();
                if (Serialization.TryFromJson(data, out UserDataOwned userDataOwned))
                {
                    SaveUserDataOwned(userDataOwned);
                }
                else
                {
                    userDataOwned = LoadUserDataOwned(false);
                }
                return userDataOwned;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private void SaveUserDataOwned(UserDataOwned userDataOwned)
        {
            if (userDataOwned?.Owned?.Count == 0)
            {
                return;
            }

            FileSystem.PrepareSaveFile(FileUserDataOwned);
            File.WriteAllText(FileUserDataOwned, Serialization.ToJson(userDataOwned));
        }

        private PriceData GetPrice(List<string> ids, string local, StoreCurrency localCurrency)
        {
            string joined = string.Join(",", ids);
            string urlPrice = string.Format(UrlApiPrice, joined, localCurrency.country.ToUpper(), localCurrency.currency.ToUpper());
            string dataPrice = Web.DownloadStringData(urlPrice).GetAwaiter().GetResult();

            Serialization.TryFromJson(dataPrice, out dynamic dataObj, out Exception ex);
            if (ex != null)
            {
                ManageException($"No price data for {joined}", ex, dataPrice.Contains("404"));
            }

            string CodeCurrency = localCurrency.currency;
            string SymbolCurrency = localCurrency.symbol;

            // When no data or error, try with USD
            if (dataObj["message"] != null && ((string)dataObj["message"]).Contains("is not supported in", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetPrice(ids, local, new StoreCurrency { country = "US", currency = "USD", symbol = "$" });
            }

            if (dataObj["message"] != null)
            {
                Logger.Info($"{dataObj["message"]}");
                return null;
            }

            return new PriceData
            {
                DataObj = dataObj,
                CodeCurrency = CodeCurrency,
                SymbolCurrency = SymbolCurrency
            };
        }

        public List<StoreCurrency> GetCurrencies()
        {
            try
            {
                if (IsUserLoggedIn)
                {
                    string response = Web.DownloadStringData(UrlUserData).GetAwaiter().GetResult();
                    if (Serialization.TryFromJson(response, out UserData userData) && userData?.Currencies != null)
                    {
                        return userData.Currencies.Select(x => new StoreCurrency { country = userData.Country, currency = x.Code.ToUpper(), symbol = x.Symbol }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, ClientName);
            }

            Logger.Warn("Used USD only");
            return new List<StoreCurrency>
            {
                new StoreCurrency { country = "US", currency = "USD", symbol = "$" }
            };
        }

        private string RemoveDescriptionPromos(string originalDescription)
        {
            if (originalDescription.IsNullOrEmpty())
            {
                return originalDescription;
            }

            // Get opening element in description. Promos are always at the start of description.
            // It has been seen that descriptions start with <a> or <div> elements
            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.Parse(originalDescription);
            INode firstChild = document.Body.FirstChild;
            if (firstChild == null || firstChild.NodeType != NodeType.Element || !firstChild.HasChildNodes)
            {
                return originalDescription;
            }

            // It's possible to check if a description has a promo if the first element contains
            // a child img element with a src that points to know promo image url patterns
            IHtmlElement htmlElement = firstChild as IHtmlElement;
            string promoUrlsRegex = @"https:\/\/items.gog.com\/(promobanners|autumn|fall|summer|winter)\/";
            bool containsPromoImage = htmlElement.QuerySelectorAll("img")
                        .Any(img => img.HasAttribute("src") && Regex.IsMatch(img.GetAttribute("src"), promoUrlsRegex, RegexOptions.IgnoreCase));
            if (!containsPromoImage)
            {
                return originalDescription;
            }

            // Remove all following <hr> and <br> elements that GOG adds after a promo
            INode nextSibling = firstChild.NextSibling;
            while (nextSibling != null && (nextSibling is IHtmlHrElement || nextSibling is IHtmlBreakRowElement))
            {
                _ = document.Body.RemoveChild(nextSibling);
                nextSibling = firstChild.NextSibling;
            }

            // Remove initial opening element and return description without promo
            _= document.Body.RemoveChild(firstChild);
            return document.Body.InnerHtml;
        }
        #endregion


        private void ManageException (string message, Exception ex, bool is404)
        {
            if (ex.Message.Contains("404") || is404)
            {
                Logger.Warn(message);
            }
            else
            {
                Common.LogError(ex, false, true, PluginName);
            }
        }
    }
}
