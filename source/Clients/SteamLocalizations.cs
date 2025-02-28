using CheckLocalizations.Models;
using CommonPluginsShared;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                AppId = SteamApi.GetAppId(game);
                if (AppId != 0)
                {
                    GameInfos gameInfos = SteamApi.GetGameInfos(AppId.ToString(), null);
                    if (gameInfos == null)
                    {
                        string message = string.Format(ResourceProvider.GetString("LOCCommonErrorGetStoreData") + Environment.NewLine + $"{game.Name} - {AppId}", "Steam");
                        Exception ex = new Exception(message);
                        Common.LogError(ex, false, true, PluginDatabase.PluginName);
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
