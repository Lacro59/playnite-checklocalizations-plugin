using CommonPluginsShared;
using CommonPluginsStores.Steam.Models.SteamKit;
using Playnite.SDK;
using Playnite.SDK.Data;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace CommonPluginsStores.Steam
{
    public class SteamKit
    {
        internal static ILogger Logger => LogManager.GetLogger();

        #region Urls
        private static string UrlAchievementImg => @"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}";

        private static string UrlApi => @"https://api.steampowered.com";
        private static string UrlGetGameAchievements => UrlApi + @"/IPlayerService/GetGameAchievements/v1/?appid={0}&language={1}";
        #endregion


        #region ISteamApps
        public static List<SteamApp> GetAppList()
        {
            try
            {
                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamApps"))
                {
                    List<SteamApp> appList = new List<SteamApp>();
                    KeyValue results = steamInterface.Call("GetAppList", 2);
                    foreach (KeyValue data in results["apps"].Children)
                    {
                        appList.Add(new SteamApp
                        {
                            AppId = data["appid"].AsUnsignedInteger(),
                            Name = data["name"].AsString()
                        });
                    }
                    return appList;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }
        #endregion

        #region ISteamUser
        public static List<SteamFriend> GetFriendList(string apiKey, ulong steamId)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["steamId"] = steamId.ToString()
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamUser", apiKey))
                {
                    List<SteamFriend> friendList = new List<SteamFriend>();
                    KeyValue results = steamInterface.Call("GetFriendList", 1, args);
                    foreach (KeyValue data in results["friends"].Children)
                    {
                        friendList.Add(new SteamFriend
                        {
                            SteamId = data["steamid"].AsUnsignedLong(),
                            Relationship = data["relationship"].AsString(),
                            FriendSince = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(data["friend_since"].AsInteger()).ToLocalTime(),
                        });
                    }
                    return friendList;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }


        public static List<SteamPlayerSummaries> GetPlayerSummaries(string apiKey, List<ulong> steamIds)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["steamIds"] = string.Join(",", steamIds)
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamUser", apiKey))
                {
                    List<SteamPlayerSummaries> friendList = new List<SteamPlayerSummaries>();
                    KeyValue results = steamInterface.Call("GetPlayerSummaries", 2, args);
                    foreach (KeyValue data in results["players"].Children)
                    {
                        friendList.Add(new SteamPlayerSummaries
                        {
                            Avatar = data["avatar"].AsString(),
                            AvatarFull = data["avatarfull"].AsString(),
                            AvatarHash = data["avatarhash"].AsString(),
                            AvatarMedium = data["avatarmedium"].AsString(),
                            CommunityVisibilityState = data["communityvisibilitystate"].AsInteger(),
                            LastLogoff = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(data["lastlogoff"].AsInteger()).ToLocalTime(),
                            LocCountryCode = data["loccountrycode"].AsString(),
                            PersonaName = data["personaname"].AsString(),
                            PersonaState = data["personastate"].AsInteger(),
                            PersonaStateFlags = data["personastateflags"].AsInteger(),
                            PrimaryClanId = data["primaryclanid"].AsString(),
                            ProfileState = data["profilestate"].AsInteger(),
                            ProfileUrl = data["profileurl"].AsString(),
                            SteamId = data["steamid"].AsString(),
                            TimeCreated = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(data["timecreated"].AsInteger()).ToLocalTime()
                        });
                    }
                    return friendList;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }
        #endregion

        #region IPlayerService
        public static List<SteamOwnedGame> GetOwnedGames(string apiKey, ulong steamId)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["steamId"] = steamId.ToString()
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("IPlayerService", apiKey))
                {
                    List<SteamOwnedGame> ownedGames = new List<SteamOwnedGame>();
                    KeyValue results = steamInterface.Call("GetOwnedGames", 1, args);
                    foreach (KeyValue data in results["games"].Children)
                    {
                        ownedGames.Add(new SteamOwnedGame
                        {
                            Appid = data["appid"].AsInteger(),
                            PlaytimeDeckForever = data["playtime_deck_forever"].AsInteger(),
                            PlaytimeDisconnected = data["playtime_disconnected"].AsInteger(),
                            PlaytimeForever = data["playtime_forever"].AsInteger(),
                            Playtime2weeks = data["playtime_2weeks"].AsInteger(),
                            PlaytimeLinuxForever = data["playtime_linux_forever"].AsInteger(),
                            PlaytimeMacForever = data["playtime_mac_forever"].AsInteger(),
                            PlaytimeWindowsForever = data["playtime_windows_forever"].AsInteger(),
                            RtimeLastPlayed = data["rtime_last_played"].AsInteger() == 0 ? default : new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(data["rtime_last_played"].AsInteger()).ToLocalTime()
                        });
                    }
                    return ownedGames;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }


        public static List<SteamAchievements> GetGameAchievements(uint appId)
        {
            return GetGameAchievements(appId, "english");
        }

        public static List<SteamAchievements> GetGameAchievements(uint appId, string language)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["appId"] = appId.ToString(),
                    ["language"] = language
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("IPlayerService"))
                {
                    List<SteamAchievements> achievementLists = new List<SteamAchievements>();
                    KeyValue results = steamInterface.Call("GetGameAchievements", 1, args);

                    // By moment, SteamKit return nothing
                    if (results["achievements"].Children.Count == 0)
                    {
                        string data = Web.DownloadStringData(string.Format(UrlGetGameAchievements, appId, language)).GetAwaiter().GetResult();
                        if (Serialization.TryFromJson(data, out Models.SteamAchievements steamAchievements))
                        {
                            steamAchievements.Response?.Achievements?.ForEach(x =>
                            {
                                achievementLists.Add(new SteamAchievements
                                {
                                    Hidden = x.Hidden,
                                    Icon = x.Icon.IsNullOrEmpty() ? string.Empty : string.Format(UrlAchievementImg, appId, x.Icon),
                                    IconGray = x.IconGray.IsNullOrEmpty() ? string.Empty : string.Format(UrlAchievementImg, appId, x.IconGray),
                                    InternalName = x.InternalName,
                                    LocalizedDesc = x.LocalizedDesc,
                                    LocalizedName = x.LocalizedName,
                                    PlayerPercentUnlocked = x.PlayerPercentUnlocked.IsNullOrEmpty() ? 100 : float.Parse(x.PlayerPercentUnlocked.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)),
                                });
                            });
                        }

                        if (achievementLists.Count > 0)
                        {
                            Common.LogDebug(true, $"GetGameAchievements by old method for {appId}");
                        }
                    }
                    else
                    {
                        foreach (KeyValue data in results["achievements"].Children)
                        {
                            achievementLists.Add(new SteamAchievements
                            {
                                Hidden = data["hidden"].AsBoolean(),
                                Icon = string.IsNullOrEmpty(data["icon"].AsString()) ? string.Empty : string.Format(UrlAchievementImg, appId, data["icon"].AsString()),
                                IconGray = string.IsNullOrEmpty(data["icon_gray"].AsString()) ? string.Empty : string.Format(UrlAchievementImg, appId, data["icon_gray"].AsString()),
                                InternalName = data["internal_name"].AsString(),
                                LocalizedDesc = data["localized_desc"].AsString(),
                                LocalizedName = data["localized_name"].AsString(),
                                PlayerPercentUnlocked = string.IsNullOrEmpty(data["player_percent_unlocked"].AsString()) ? 100 : float.Parse(data["player_percent_unlocked"].AsString().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)),
                            });
                        }
                    }
                    return achievementLists;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"With {appId}");
                return null;
            }
        }
        #endregion

        #region ISteamUserStats
        public static SteamSchema GetSchemaForGame(string apiKey, uint appId)
        {
            return GetSchemaForGame(apiKey, appId, "english");
        }

        public static SteamSchema GetSchemaForGame(string apiKey, uint appId, string language)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["appId"] = appId.ToString(),
                    ["l"] = language
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamUserStats", apiKey))
                {
                    SteamSchema schemaForGame = new SteamSchema();
                    KeyValue results = steamInterface.Call("GetSchemaForGame", 2, args);
                    foreach (KeyValue data in results["availableGameStats"]["stats"].Children)
                    {
                        schemaForGame.Stats.Add(new SteamSchemaStats
                        {
                            Name = data["name"].AsString(),
                            DefaultValue = data["defaultvalue"].AsInteger(),
                            DisplayName = data["displayName"].AsString()
                        });
                    }
                    foreach (KeyValue data in results["availableGameStats"]["achievements"].Children)
                    {
                        schemaForGame.Achievements.Add(new SteamSchemaAchievements
                        {
                            Name = data["name"].AsString(),
                            DefaultValue = data["defaultvalue"].AsInteger(),
                            DisplayName = data["displayName"].AsString(),
                            Hidden = data["hidden"].AsBoolean(),
                            Icon = data["icon"].AsString(),
                            IconGray = data["icongray"].AsString()
                        });
                    }
                    return schemaForGame;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"With {appId}");
                return null;
            }
        }


        public static List<SteamStats> GetUserStatsForGame(string apiKey, uint appId, ulong steamId)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["appId"] = appId.ToString(),
                    ["steamId"] = steamId.ToString()
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamUserStats", apiKey))
                {
                    List<SteamStats> userStatsForGames = new List<SteamStats>();
                    KeyValue results = steamInterface.Call("GetUserStatsForGame", 1, args);
                    foreach (KeyValue data in results["stats"].Children)
                    {
                        userStatsForGames.Add(new SteamStats
                        {
                            Name = data.Name,
                            Value = data["value"].AsString()
                        });
                    }
                    return userStatsForGames;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"With {appId}");
                return null;
            }
        }


        public static List<SteamPlayerAchievement> GetPlayerAchievements(string apiKey, uint appId, ulong steamId)
        {
            return GetPlayerAchievements(apiKey, appId, steamId, "english");
        }

        public static List<SteamPlayerAchievement> GetPlayerAchievements(string apiKey, uint appId, ulong steamId, string language)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["appId"] = appId.ToString(),
                    ["steamId"] = steamId.ToString(),
                    ["l"] = language
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamUserStats", apiKey))
                {
                    List<SteamPlayerAchievement> steamPlayerAchievements = new List<SteamPlayerAchievement>();
                    KeyValue results = steamInterface.Call("GetPlayerAchievements", 1, args);

                    if (results["achievements"].Children.Count == 0)
                    {
                        //if (Serialization.TryFromJson(data, out dynamic steamAchievements))
                        //string data = Web.DownloadStringData(string.Format(UrlGetPlayerAchievements, appId, language, apiKey, steamId)).GetAwaiter().GetResult();
                        //{
                        //    foreach (dynamic dataApi in steamAchievements["playerstats"]["achievements"])
                        //    {
                        //        steamPlayerAchievements.Add(new SteamPlayerAchievement
                        //        {
                        //            Achieved = (int)dataApi["achieved"],
                        //            ApiName = dataApi["apiname"],
                        //            Description = dataApi["description"],
                        //            Name = dataApi["name"],
                        //            UnlockTime = (int)dataApi["unlocktime"] == 0 ? default : new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((int)dataApi["unlocktime"]).ToLocalTime()
                        //        });
                        //    }
                        //}
                    }
                    else
                    {
                        foreach (KeyValue data in results["achievements"].Children)
                        {
                            steamPlayerAchievements.Add(new SteamPlayerAchievement
                            {
                                Achieved = data["achieved"].AsInteger(),
                                ApiName = data["apiname"].AsString(),
                                Description = data["description"].AsString(),
                                Name = data["name"].AsString(),
                                UnlockTime = data["unlocktime"].AsInteger() == 0 ? default : new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(data["unlocktime"].AsInteger()).ToLocalTime()
                            });
                        }
                    }
                    return steamPlayerAchievements;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"With {appId}");
                return null;
            }
        }

        public static bool CheckGameIsPrivate(string apiKey, uint appId, ulong steamId)
        {
            try
            {
                Logger.Info($"CheckGameIsPrivate({appId})");
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    ["appId"] = appId.ToString(),
                    ["steamId"] = steamId.ToString(),
                    ["l"] = "english"
                };

                using (WebAPI.Interface steamInterface = WebAPI.GetInterface("ISteamUserStats", apiKey))
                {
                    List<SteamPlayerAchievement> steamPlayerAchievements = new List<SteamPlayerAchievement>();
                    KeyValue results = steamInterface.Call("GetPlayerAchievements", 1, args);
                    return false;
                }
            }
            catch (Exception ex)
            {
                return ex.Message.Contains("403");
            }
        }
        #endregion
    }
}
