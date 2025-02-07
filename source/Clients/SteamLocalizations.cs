using CheckLocalizations.Models;
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
        private static ILogger Logger => LogManager.GetLogger();

        private static LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;

        private SteamApi SteamApi { get; set; }
        private uint AppId { get; set; }


        public SteamLocalizations()
        {
            SteamApi = new SteamApi(PluginDatabase.PluginName, PlayniteTools.ExternalPlugin.CheckLocalizations);
        }


        public List<Localization> GetLocalizations(Game game)
        {
            List<Localization> localizations = new List<Localization>();

            try
            {
                AppId = SteamApi.GetAppId(game);
                if (AppId != 0)
                {
                    string data = GetSteamData();
                    if (data.Contains("\"success\":false", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string message = string.Format(ResourceProvider.GetString("LOCCommonErrorGetStoreData") + Environment.NewLine + $"{game.Name} - {AppId}", "Steam");
                        Exception ex = new Exception(message);
                        Common.LogError(ex, false, true, PluginDatabase.PluginName);
                        return localizations;
                    }

                    if (Serialization.TryFromJson(data, out Dictionary<string, StoreAppDetailsResult> parsedData) && parsedData[AppId.ToString()].data != null)
                    {
                        string[] dataSplited = parsedData[AppId.ToString()].data.supported_languages.Split(new string[] { "<br>" }, StringSplitOptions.None);
                        string[] listLocalizations = dataSplited[0].Split(',');

                        foreach(string Loc in listLocalizations)
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

                            localizations.Add(new Localization
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
                    Logger.Warn($"Not find for {game.Name}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error with {game.Name} - {AppId}", true, PluginDatabase.PluginName);
            }

            return localizations;
        }


        private string GetSteamData()
        {
            string url = string.Empty;
            try
            {
                url = $"https://store.steampowered.com/api/appdetails?appids={AppId}&l=english";
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
            return $"https://store.steampowered.com/app/{AppId}/";
        }

        public string GetGameName()
        {
            return SteamApi.GetGameName(AppId);
        }
    }
}
