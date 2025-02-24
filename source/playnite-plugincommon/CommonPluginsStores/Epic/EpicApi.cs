using CommonPlayniteShared.Common;
using CommonPlayniteShared.PluginLibrary.EpicLibrary.Models;
using CommonPlayniteShared.PluginLibrary.EpicLibrary.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Models;
using CommonPluginsStores.Epic.Models;
using CommonPluginsStores.Epic.Models.Query;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommonPlayniteShared.PluginLibrary.EpicLibrary.Models.WebStoreModels.QuerySearchResponse.Data.CatalogItem.SearchStore;
using static CommonPluginsShared.PlayniteTools;

namespace CommonPluginsStores.Epic
{
    // https://gist.github.com/woctezuma/8ca464a276b15d7dfad475fd6b6cbee9
    public class EpicApi : StoreApi
    {
        #region Url
        private string UrlBase => @"https://www.epicgames.com";
        private string UrlStore => UrlBase + @"/store/{0}/p/{1}";
        private string UrlAchievements => UrlBase + @"/store/{0}/achievements/{1}";
        private string UrlLogin => UrlBase + @"/id/login?redirectUrl=https%3A//www.epicgames.com/id/api/redirect%3FclientId%3D34a02cf8f4414e29b15921876da36f9a%26responseType%3Dcode";

        private string UrlGraphQL => @"https://graphql.epicgames.com/graphql";

        private string UrlApiServiceBase => @"https://account-public-service-prod03.ol.epicgames.com";
        private string UrlAccountAuth => UrlApiServiceBase + @"/account/api/oauth/token";
        private string UrlAccount => UrlApiServiceBase + @"/account/api/public/account/{0}";
        private string UrlStoreEpic => @"https://store.epicgames.com";
        private string UrlAccountProfile => UrlStoreEpic + @"/u/{0}";
        private string UrlAccountLinkFriends => UrlStoreEpic + @"/u/{0}/friends";
        private string UrlAccountAchievementss => UrlStoreEpic + @"/{0}/u/{1}/details/{2}";
        #endregion

        private static string AuthEncodedString => "MzRhMDJjZjhmNDQxNGUyOWIxNTkyMTg3NmRhMzZmOWE6ZGFhZmJjY2M3Mzc3NDUwMzlkZmZlNTNkOTRmYzc2Y2Y=";

        #region Paths
        private string TokensPath { get; }
        #endregion


        public EpicApi(string pluginName, ExternalPlugin pluginLibrary) : base(pluginName, pluginLibrary, "Epic")
        {
            TokensPath = Path.Combine(PathStoresData, "Epic_Tokens.dat");
        }

        #region Cookies
        internal override List<HttpCookie> GetWebCookies()
        {
            string LocalLangShort = CodeLang.GetEpicLangCountry(Local);
            List<HttpCookie> httpCookies = new List<HttpCookie>
            {
                new HttpCookie
                {
                    Domain = ".www.epicgames.com",
                    Name = "EPIC_LOCALE_COOKIE",
                    Value = LocalLangShort
                },
                new HttpCookie
                {
                    Domain = ".www.epicgames.com",
                    Name = "EPIC_EG1",
                    Value = AuthToken?.Token ?? string.Empty
                },
                new HttpCookie
                {
                    Domain = "store.epicgames.com",
                    Name = "EPIC_LOCALE_COOKIE",
                    Value = LocalLangShort
                },
                new HttpCookie
                {
                    Domain = "store.epicgames.com",
                    Name = "EPIC_EG1",
                    Value = AuthToken?.Token ?? string.Empty
                }
            };
            return httpCookies;
        }
        #endregion

        #region Configuration
        protected override bool GetIsUserLoggedIn()
        {
            if (!_currentAccountInfos.IsPrivate && !StoreSettings.UseAuth)
            {
                return !_currentAccountInfos.UserId.IsNullOrEmpty();
            }

            bool isLogged = CheckIsUserLoggedIn();
            if (isLogged)
            {
                OauthResponse tokens = LoadTokens();
                AuthToken = new StoreToken
                {
                    Token = tokens.access_token,
                    Type = tokens.token_type
                };

                _ = SetStoredCookies(GetNewWebCookies(UrlStoreEpic));
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
                EpicLogin();

                OauthResponse tokens = LoadTokens();
                if (tokens != null)
                {
                    List<HttpHeader> httpHeaders = new List<HttpHeader>
                    {
                        new HttpHeader { Key = "Authorization", Value = tokens.token_type + " " + tokens.access_token }
                    };
                    string response = Web.DownloadStringData(string.Format(UrlAccount, tokens.account_id), httpHeaders).GetAwaiter().GetResult();
                    if (Serialization.TryFromJson(response, out EpicAccountResponse epicAccountResponse))
                    {
                        CurrentAccountInfos = new AccountInfos
                        {
                            UserId = tokens.account_id,
                            Pseudo = epicAccountResponse?.DisplayName,
                            Link = string.Format(UrlAccountProfile, tokens.account_id),
                            IsPrivate = true,
                            IsCurrent = true
                        };
                        SaveCurrentUser();
                        _ = GetCurrentAccountInfos();

                        Logger.Info($"{PluginName} logged");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }
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
                    CurrentAccountInfos.IsPrivate = !CheckIsPublic(accountInfos).GetAwaiter().GetResult();
                    CurrentAccountInfos.AccountStatus = CurrentAccountInfos.IsPrivate ? AccountStatus.Private : AccountStatus.Public;
                });
                return accountInfos;
            }
            return new AccountInfos { IsCurrent = true };
        }
        #endregion

        #region User details
        // TODO
        public override ObservableCollection<AccountGameInfos> GetAccountGamesInfos(AccountInfos accountInfos)
        {
            if (!IsUserLoggedIn)
            {
                return null;
            }

            try
            {
                ObservableCollection<AccountGameInfos> accountGamesInfos = new ObservableCollection<AccountGameInfos>();
                return accountGamesInfos;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }

        /// <summary>
        /// Get a list of a game's achievements with a user's possessions.
        /// </summary>
        /// <param name="id">nameSpace</param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public override ObservableCollection<GameAchievement> GetAchievements(string id, AccountInfos accountInfos)
        {
            if (!IsUserLoggedIn)
            {
                return null;
            }

            try
            {
                ObservableCollection<GameAchievement> gameAchievements = new ObservableCollection<GameAchievement>();

                string url = string.Empty;
                string resultWeb = string.Empty;
                string localLang = CodeLang.GetEpicLang(Local);
                string localLangShort = CodeLang.GetEpicLangCountry(Local);

                // Get Achievement game schema
                EpicAchievementResponse epicAchievementResponse = QueryAchievement(id, localLangShort).GetAwaiter().GetResult();
                string productId = epicAchievementResponse.Data?.Achievement?.ProductAchievementsRecordBySandbox?.ProductId;
                epicAchievementResponse?.Data?.Achievement?.ProductAchievementsRecordBySandbox?.Achievements?.ForEach(x =>
                {
                    GameAchievement gameAchievement = new GameAchievement
                    {
                        Id = x.Achievement.Name,
                        Name = x.Achievement.UnlockedDisplayName,
                        Description = x.Achievement.UnlockedDescription,
                        UrlUnlocked = x.Achievement.UnlockedIconLink,
                        UrlLocked = x.Achievement.LockedIconLink,
                        DateUnlocked = default,
                        Percent = x.Achievement.Rarity.Percent,
                        GamerScore = x.Achievement.XP
                    };
                    gameAchievements.Add(gameAchievement);
                });

                if (!accountInfos.IsPrivate && !StoreSettings.UseAuth)
                {
                    EpicPlayerProfileAchievementsByProductIdResponse playerProfileAchievementsByProductId = QueryPlayerProfileAchievementsByProductId(accountInfos.UserId, productId).GetAwaiter().GetResult();
                    playerProfileAchievementsByProductId?.Data?.PlayerProfile?.PlayerProfile2?.ProductAchievements?.Data?.PlayerAchievements?.ForEach(x =>
                    {
                        GameAchievement owned = gameAchievements.Where(y => y.Id.IsEqual(x.PlayerAchievement.AchievementName))?.FirstOrDefault();
                        if (owned != null)
                        {
                            owned.DateUnlocked = x?.PlayerAchievement.UnlockDate ?? default;
                        }
                    });
                }
                else
                {
                    EpicPlayerAchievementResponse epicPlayerAchievementResponse = QueryPlayerAchievement(accountInfos.UserId, id).GetAwaiter().GetResult();
                    epicPlayerAchievementResponse?.Data?.PlayerAchievement?.PlayerAchievementGameRecordsBySandbox?.Records?.FirstOrDefault().PlayerAchievements.ForEach(x =>
                    {
                        GameAchievement owned = gameAchievements.Where(y => y.Id.IsEqual(x.PlayerAchievement.AchievementName))?.FirstOrDefault();
                        if (owned != null)
                        {
                            owned.DateUnlocked = x.PlayerAchievement?.UnlockDate ?? default;
                        }
                    });
                }

                return gameAchievements;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }


        /// <summary>
        /// Get achievements SourceLink.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id">productSlug</param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public override SourceLink GetAchievementsSourceLink(string name, string id, AccountInfos accountInfos)
        {
            string LocalLang = CodeLang.GetEpicLang(Local);
            string Url = string.Format(UrlAchievements, LocalLang, id);

            return new SourceLink
            {
                GameName = name,
                Name = ClientName,
                Url = Url
            };
        }

        public override ObservableCollection<AccountWishlist> GetWishlist(AccountInfos accountInfos)
        {
            if (accountInfos != null)
            {
                try
                {
                    string query = "query wishlistQuery { Wishlist { wishlistItems { elements { id order created offerId updated namespace offer {id title offerType effectiveDate expiryDate status isCodeRedemptionOnly keyImages { type url width height }catalogNs { mappings(pageType: \"productHome\") { pageSlug pageType } } offerMappings { pageSlug pageType } } } } } }";
                    dynamic variables = new { };
                    string response = QueryWishList(query, variables).GetAwaiter().GetResult();

                    if (!response.IsNullOrEmpty() && Serialization.TryFromJson(response, out EpicWishlistData epicWishlistData))
                    {
                        if (epicWishlistData?.data?.Wishlist?.wishlistItems?.elements != null)
                        {
                            ObservableCollection<AccountWishlist> data = new ObservableCollection<AccountWishlist>();

                            foreach (WishlistElement gameWishlist in epicWishlistData.data.Wishlist.wishlistItems.elements)
                            {
                                string Id = string.Empty;
                                string Name = string.Empty;
                                DateTime? Released = null;
                                DateTime? Added = null;
                                string Image = string.Empty;
                                string Link = string.Empty;

                                try
                                {
                                    Id = gameWishlist.offerId + "|" + gameWishlist.@namespace;
                                    Name = WebUtility.HtmlDecode(gameWishlist.offer.title);
                                    Image = gameWishlist.offer.keyImages?.FirstOrDefault(x => x.type.IsEqual("Thumbnail"))?.url;
                                    Released = gameWishlist.offer.effectiveDate.ToUniversalTime();
                                    Added = gameWishlist.created.ToUniversalTime();
                                    Link = gameWishlist.offer?.catalogNs?.mappings?.FirstOrDefault()?.pageSlug;

                                    data.Add(new AccountWishlist
                                    {
                                        Id = Id,
                                        Name = Name,
                                        Link = Link.IsNullOrEmpty() ? string.Empty : string.Format(UrlStore, CodeLang.GetEpicLang(Local), Link),
                                        Released = Released,
                                        Added = Added,
                                        Image = Image
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Common.LogError(ex, true, $"Error in parse {ClientName} wishlist - {Name}");
                                }
                            }

                            return data;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error in parse {ClientName} wishlist", true, PluginName);
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
                    string EpicOfferId = id.Split('|')[0];
                    string EpicNamespace = id.Split('|')[1];

                    string query = @"mutation removeFromWishlistMutation($namespace: String!, $offerId: String!, $operation: RemoveOperation!) { Wishlist { removeFromWishlist(namespace: $namespace, offerId: $offerId, operation: $operation) { success } } }";
                    dynamic variables = new
                    {
                        @namespace = EpicNamespace,
                        offerId = EpicOfferId,
                        operation = "REMOVE"
                    };
                    string ResultWeb = QueryWishList(query, variables).GetAwaiter().GetResult();
                    return ResultWeb.IndexOf("\"success\":true") > -1;
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
        // TODO
        public override GameInfos GetGameInfos(string id, AccountInfos accountInfos)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }

        /// <summary>
        /// Get dlc informations for a game.
        /// </summary>
        /// <param name="id">epic_namespace</param>
        /// <param name="accountInfos"></param>
        /// <returns></returns>
        public override ObservableCollection<DlcInfos> GetDlcInfos(string id, AccountInfos accountInfos)
        {
            try
            {
                string LocalLang = CodeLang.GetEpicLang(Local);
                ObservableCollection<DlcInfos> Dlcs = new ObservableCollection<DlcInfos>();

                // List DLC
                EpicAddonsByNamespace dataDLC = GetAddonsByNamespace(id).GetAwaiter().GetResult();
                if (dataDLC?.data?.Catalog?.catalogOffers?.elements == null)
                {
                    Logger.Warn($"No dlc for {id}");
                    return null;
                }

                foreach (Element el in dataDLC?.data?.Catalog?.catalogOffers?.elements)
                {
                    bool IsOwned = false;
                    if (accountInfos != null && accountInfos.IsCurrent)
                    {
                        IsOwned = DlcIsOwned(id, el.id);
                    }

                    DlcInfos dlc = new DlcInfos
                    {
                        Id = el.id,
                        Name = el.title,
                        Description = el.description,
                        Image = el.keyImages?.Find(x => x.type.IsEqual("OfferImageWide"))?.url?.Replace("\u002F", "/"),
                        Link = string.Format(UrlStore, LocalLang, el.urlSlug),
                        IsOwned = IsOwned,
                        Price = el.price?.totalPrice?.fmtPrice?.discountPrice,
                        PriceBase = el.price?.totalPrice?.fmtPrice?.originalPrice,
                    };

                    Dlcs.Add(dlc);
                }

                return Dlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return null;
        }
        #endregion

        #region Epic
        private OauthResponse LoadTokens()
        {
            if (File.Exists(TokensPath))
            {
                try
                {
                    return Serialization.FromJson<OauthResponse>(
                        Encryption.DecryptFromFile(
                            TokensPath,
                            Encoding.UTF8,
                            WindowsIdentity.GetCurrent().User.Value));
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, false, PluginName);
                }
            }

            return null;
        }

        private EpicAccountResponse GetEpicAccount()
        {
            OauthResponse tokens = LoadTokens();
            if (tokens != null)
            {
                List<HttpHeader> httpHeaders = new List<HttpHeader>
                    {
                        new HttpHeader { Key = "Authorization", Value = tokens.token_type + " " + tokens.access_token }
                    };
                string response = Web.DownloadStringData(string.Format(UrlAccount, tokens.account_id), httpHeaders).GetAwaiter().GetResult();
                if (Serialization.TryFromJson(response, out EpicAccountResponse epicAccountResponse))
                {
                    return epicAccountResponse;
                }
            }

            return null;
        }

        public async Task<bool> CheckIsPublic(AccountInfos accountInfos)
        {
            try
            {
                string url = string.Format(UrlAccountProfile, accountInfos.UserId);
                string response = await Web.DownloadStringData(url);
                return !response.Contains("private-view-text");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return false;
        }

        private bool CheckIsUserLoggedIn()
        {
            OauthResponse tokens = LoadTokens();
            if (tokens == null)
            {
                return false;
            }

            try
            {
                EpicAccountResponse account = GetEpicAccount();
                return account != null && account.Id == tokens.account_id;
            }
            catch
            {
                try
                {
                    RenewTokens(tokens.refresh_token);
                    tokens = LoadTokens();
                    if (tokens.account_id.IsNullOrEmpty() || tokens.access_token.IsNullOrEmpty())
                    {
                        return false;
                    }

                    EpicAccountResponse account = GetEpicAccount();
                    return account.Id == tokens.account_id;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, "Failed to validation Epic authentication.", false, PluginName);
                    return false;
                }
            }
        }

        private void EpicLogin()
        {
            bool loggedIn = false;
            string apiRedirectContent = string.Empty;

            using (IWebView view = API.Instance.WebViews.CreateView(new WebViewSettings
            {
                WindowWidth = 580,
                WindowHeight = 700,
                // This is needed otherwise captcha won't pass
                UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 Vivaldi/5.5.2805.50"
            }))
            {
                view.LoadingChanged += async (s, e) =>
                {
                    string address = view.GetCurrentAddress();
                    if (address.StartsWith(@"https://www.epicgames.com/id/api/redirect"))
                    {
                        apiRedirectContent = await view.GetPageTextAsync();
                        loggedIn = true;
                        view.Close();
                    }
                };

                view.DeleteDomainCookies(".epicgames.com");
                view.Navigate(UrlLogin);
                _ = view.OpenDialog();
            }

            if (!loggedIn)
            {
                return;
            }

            if (apiRedirectContent.IsNullOrEmpty())
            {
                return;
            }

            string authorizationCode = Serialization.FromJson<ApiRedirectResponse>(apiRedirectContent).authorizationCode;
            FileSystem.DeleteFile(TokensPath);
            if (string.IsNullOrEmpty(authorizationCode))
            {
                Logger.Error("Failed to get login exchange key for Epic account.");
                return;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", "basic " + AuthEncodedString);
                using (StringContent content = new StringContent($"grant_type=authorization_code&code={authorizationCode}&token_type=eg1"))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    HttpResponseMessage response = httpClient.PostAsync(UrlAccountAuth, content).GetAwaiter().GetResult();
                    string respContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    FileSystem.CreateDirectory(Path.GetDirectoryName(TokensPath));
                    Encryption.EncryptToFile(
                        TokensPath,
                        respContent,
                        Encoding.UTF8,
                        WindowsIdentity.GetCurrent().User.Value);
                }
            }
        }

        private void RenewTokens(string refreshToken)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", "basic " + AuthEncodedString);
                using (StringContent content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}&token_type=eg1"))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    HttpResponseMessage response = httpClient.PostAsync(UrlAccountAuth, content).GetAwaiter().GetResult();
                    string respContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    FileSystem.CreateDirectory(Path.GetDirectoryName(TokensPath));
                    Encryption.EncryptToFile(
                        TokensPath,
                        respContent,
                        Encoding.UTF8,
                        WindowsIdentity.GetCurrent().User.Value);
                }
            }
        }


        public string GetProducSlug(Game game)
        {
            string productSlug = string.Empty;
            string normalizedEpicName = PlayniteTools.NormalizeGameName(game.Name.Replace("'", "").Replace(",", ""));
            game.Links?.ForEach(x =>
            {
                productSlug = GetProductSlugByUrl(x.Url, normalizedEpicName).IsNullOrEmpty() ? productSlug : GetProductSlugByUrl(x.Url, normalizedEpicName);
            });

            if (productSlug.IsNullOrEmpty())
            {
                productSlug = GetProductSlug(normalizedEpicName);
            }

            if (productSlug.IsNullOrEmpty())
            {
                Logger.Warn($"No ProductSlug for {game.Name}");
            }

            return productSlug;
        }

        private string GetProductSlug(string name)
        {
            if (name.IsEqual("warhammer 40 000 mechanicus"))
            {
                name = "warhammer mechanicus";
            }
            else if (name.IsEqual("death stranding"))
            {
                return "death-stranding";
            }

            string ProductSlug = string.Empty;

            try
            {
                using (WebStoreClient client = new WebStoreClient())
                {
                    List<SearchStoreElement> catalogs = client.QuerySearch(name).GetAwaiter().GetResult();
                    if (catalogs.HasItems())
                    {
                        catalogs = catalogs.OrderBy(x => x.title.Length).ToList();
                        SearchStoreElement catalog = catalogs.FirstOrDefault(a => a.title.IsEqual(name, true));
                        if (catalog == null)
                        {
                            catalog = catalogs[0];
                        }

                        if (catalog.productSlug.IsNullOrEmpty())
                        {
                            SearchStoreElement.CatalogNs.Mappings mapping = catalog.catalogNs.mappings.FirstOrDefault(b => b.pageType.Equals("productHome", StringComparison.InvariantCultureIgnoreCase));
                            catalog.productSlug = mapping.pageSlug;
                        }

                        ProductSlug = catalog?.productSlug?.Replace("/home", string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return ProductSlug;
        }

        private string GetProductSlugByUrl(string url, string gameName)
        {
            string ProductSlug = string.Empty;

            if (url.Contains("store.epicgames.com", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    string[] urlSplit = url.Split('/');
                    foreach (string slug in urlSplit)
                    {
                        if (slug.ContainsInvariantCulture(gameName.ToLower(), System.Globalization.CompareOptions.IgnoreSymbols))
                        {
                            ProductSlug = slug;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            return ProductSlug;
        }

        private string GetNameSpace(string name)
        {
            return GetNameSpace(name, string.Empty);
        }

        private string GetNameSpace(string name, string productSlug)
        {
            string nameSpace = string.Empty;

            try
            {
                using (WebStoreClient client = new WebStoreClient())
                {
                    List<SearchStoreElement> catalogs = client.QuerySearch(name).GetAwaiter().GetResult();
                    if (catalogs.HasItems())
                    {
                        catalogs = catalogs.OrderBy(x => x.title.Length).ToList();

                        SearchStoreElement catalog = null;
                        if (productSlug.IsNullOrEmpty())
                        {
                            catalog = catalogs.FirstOrDefault(a => a.title.IsEqual(name, true)); }
                        else
                        {
                            catalog = catalogs.FirstOrDefault(a => a.productSlug.IsEqual(productSlug, true));
                            if (catalog == null)
                            {
                                catalogs.ForEach(x =>
                                {
                                    if (catalog == null)
                                    {
                                        SearchStoreElement.CatalogNs.Mappings found = x?.catalogNs?.mappings?.FirstOrDefault(b => b?.pageSlug?.IsEqual(productSlug) ?? false);
                                        if (found != null)
                                        {
                                            catalog = x;
                                        }
                                    }
                                });
                            }
                        }

                        if (catalog == null)
                        {
                            Logger.Warn($"Not find nameSpace for {name} - {productSlug}");
                        }

                        nameSpace = catalog?.nameSpace;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return nameSpace;
        }

        public string GetNameSpace(Game game)
        {
            string productSlug = GetProducSlug(game);
            string normalizedEpicName = PlayniteTools.NormalizeGameName(game.Name.Replace("'", "").Replace(",", ""));

            // The search don't find the classic game
            if (productSlug == "death-stranding")
            {
                return "f4a904fcef2447439c35c4e6457f3027";
            }
            return productSlug.IsNullOrEmpty() ? GetNameSpace(normalizedEpicName) : GetNameSpace(normalizedEpicName, productSlug);
        }

        private bool DlcIsOwned(string productNameSpace, string id)
        {
            try
            {
                EpicEntitledOfferItems ownedDLC = GetEntitledOfferItems(productNameSpace, id).GetAwaiter().GetResult();
                return (ownedDLC?.data?.Launcher?.entitledOfferItems?.entitledToAllItemsInOffer ?? false) && (ownedDLC?.data?.Launcher?.entitledOfferItems?.entitledToAnyItemInOffer ?? false);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return false;
            }
        }


        private async Task<EpicAddonsByNamespace> GetAddonsByNamespace(string epic_namespace)
        {
            try
            {
                QueryAddonsByNamespace query = new QueryAddonsByNamespace();
                query.variables.epic_namespace = epic_namespace;
                query.variables.locale = CodeLang.GetEpicLang(Local);
                query.variables.country = CodeLang.GetOriginLangCountry(Local);
                StringContent content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.PostAsync(UrlGraphQL, content);
                string str = await response.Content.ReadAsStringAsync();
                EpicAddonsByNamespace data = Serialization.FromJson<EpicAddonsByNamespace>(str);
                return data;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private async Task<EpicEntitledOfferItems> GetEntitledOfferItems(string productNameSpace, string offerId)
        {
            try
            {
                QueryEntitledOfferItems query = new QueryEntitledOfferItems();
                query.variables.productNameSpace = productNameSpace;
                query.variables.offerId = offerId;
                StringContent content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
                string str = await Web.PostStringData(UrlGraphQL, AuthToken?.Token, content);
                EpicEntitledOfferItems data = Serialization.FromJson<EpicEntitledOfferItems>(str);
                return data;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        public async Task<string> QueryWishList(string query, dynamic variables)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken.Token);

                var queryObject = new
                {
                    query = query,
                    variables = variables
                };
                StringContent content = new StringContent(Serialization.ToJson(queryObject), Encoding.UTF8, "application/json");
                string str = await Web.PostStringData(UrlGraphQL, AuthToken.Token, content);

                return str;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private async Task<EpicAchievementResponse> QueryAchievement(string sandboxId, string locale)
        {
            try
            {
                QueryAchievement query = new QueryAchievement();
                query.variables.locale = locale;
                query.variables.sandboxId = sandboxId;
                StringContent content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
                string str = await Web.PostStringData(UrlGraphQL, content);
                EpicAchievementResponse data = Serialization.FromJson<EpicAchievementResponse>(str);
                return data;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private async Task<EpicPlayerAchievementResponse> QueryPlayerAchievement(string epicAccountId, string sandboxId)
        {
            try
            {
                if (AuthToken?.Token?.IsNullOrEmpty() ?? true)
                {
                    IsUserLoggedIn = false;
                    return null;
                }

                QueryPlayerAchievement query = new QueryPlayerAchievement();
                query.variables.epicAccountId = epicAccountId;
                query.variables.sandboxId = sandboxId;
                StringContent content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
                string str = await Web.PostStringData(UrlGraphQL, AuthToken.Token, content);
                EpicPlayerAchievementResponse data = Serialization.FromJson<EpicPlayerAchievementResponse>(str.Replace("\"unlockDate\":\"N/A\",", string.Empty));
                return data;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }

        private async Task<EpicPlayerProfileAchievementsByProductIdResponse> QueryPlayerProfileAchievementsByProductId(string epicAccountId, string productId)
        {
            try
            {
                QueryPlayerProfileAchievementsByProductId query = new QueryPlayerProfileAchievementsByProductId();
                query.variables.epicAccountId = epicAccountId;
                query.variables.productId = productId;
                StringContent content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
                string str = await Web.PostStringData(UrlGraphQL, content);
                EpicPlayerProfileAchievementsByProductIdResponse data = Serialization.FromJson<EpicPlayerProfileAchievementsByProductIdResponse>(str.Replace("\"unlockDate\":\"N/A\",", string.Empty));
                return data;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return null;
            }
        }
        #endregion
    }
}
