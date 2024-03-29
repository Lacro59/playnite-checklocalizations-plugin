﻿using CheckLocalizations.Models;
using CommonPlayniteShared.PluginLibrary.SteamLibrary.SteamShared;
using CommonPluginsShared;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using CheckLocalizations.Services;
using CommonPluginsStores.Steam;

namespace CheckLocalizations.Clients
{
    public class SteamLocalizations
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        private SteamApi steamApi;
        private int SteamId;


        public SteamLocalizations()
        {
            steamApi = new SteamApi(PluginDatabase.PluginName);
        }


        public List<Localization> GetLocalizations(Game game)
        {
            List<Localization> Localizations = new List<Localization>();

            try
            {
                SteamId = steamApi.GetAppId(game.Name);
                if (SteamId != 0)
                {
                    string data = GetSteamData(SteamId);
                    if (data.Contains("\"success\":false", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string message = string.Format(ResourceProvider.GetString("LOCCommonErrorGetStoreData") + Environment.NewLine + $"{game.Name} - {SteamId}", "Steam");
                        Exception ex = new Exception(message);
                        Common.LogError(ex, false, true, PluginDatabase.PluginName);
                        return Localizations;
                    }

                    if (Serialization.TryFromJson(data, out Dictionary<string, StoreAppDetailsResult> parsedData) && parsedData[SteamId.ToString()].data != null)
                    {
                        string[] dataSplited = parsedData[SteamId.ToString()].data.supported_languages.Split(new string[] { "<br>" }, StringSplitOptions.None);
                        string[] ListLocalizations = dataSplited[0].Split(',');

                        foreach(string Loc in ListLocalizations)
                        {
                            string Language = string.Empty;
                            SupportStatus Ui = SupportStatus.Native;
                            SupportStatus Audio = SupportStatus.Unknown;
                            SupportStatus Sub = SupportStatus.Unknown;

                            if (Loc.Contains("<strong>*</strong>"))
                            {
                                Audio = SupportStatus.Native;
                                Sub = SupportStatus.Native;
                            }
                            
                            Language = Loc.Replace("<strong>*</strong>", string.Empty).Trim();

                            switch (Language)
                            {
                                case "Portuguese - Brazil":
                                    Language = "Brazilian Portuguese";
                                    break;
                                case "Spanish - Spain":
                                    Language = "Spanish";
                                    break;
                                default:
                                    break;
                            }

                            Localizations.Add(new Localization
                            {
                                Language = Language,
                                Ui = Ui,
                                Audio = Audio,
                                Sub = Sub,
                                Notes = string.Empty,
                                IsManual = false
                            });
                        }
                    }
                }
                else
                {
                    logger.Warn($"Not find for {game.Name}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error with {game.Name} - {SteamId}", true, PluginDatabase.PluginName);
            }

            return Localizations;
        }


        private string GetSteamData(int SteamId)
        {
            string url = string.Empty;
            try
            {
                url = $"https://store.steampowered.com/api/appdetails?appids={SteamId}&l=english";
                return Web.DownloadStringData(url).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Failed to download {url}", true, PluginDatabase.PluginName);
                return string.Empty;
            }
        }


        public string GetUrl()
        {
            return $"https://store.steampowered.com/app/{SteamId}/";
        }

        public string GetGameName()
        {
            return steamApi.GetGameName(SteamId);
        }
    }
}
