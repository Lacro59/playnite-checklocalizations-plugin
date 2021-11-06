using CheckLocalizations.Models;
using CommonPlayniteShared.PluginLibrary.SteamLibrary.SteamShared;
using CommonPluginsShared;
using CommonPluginsStores;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Clients
{
    public class SteamLocalizations
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _PlayniteApi;

        private readonly string _PluginUserDataPath;

        private SteamApi steamApi;
        private int SteamId;


        public SteamLocalizations(IPlayniteAPI PlayniteApi, string PluginUserDataPath)
        {
            _PlayniteApi = PlayniteApi;
            _PluginUserDataPath = PluginUserDataPath;

            steamApi = new SteamApi();
        }


        public List<Localization> GetLocalizations(Game game)
        {
            List<Localization> Localizations = new List<Localization>();

            try
            {
                SteamId = steamApi.GetSteamId(game.Name);

                if (SteamId != 0)
                {
                    string data = GetSteamData(SteamId);
                    var parsedData = Serialization.FromJson<Dictionary<string, StoreAppDetailsResult>>(data);

                    if (parsedData[SteamId.ToString()].data != null)
                    {
                        var dataSplited = parsedData[SteamId.ToString()].data.supported_languages.Split(new string[] { "<br>" }, StringSplitOptions.None);
                        var ListLocalizations = dataSplited[0].Split(',');

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
                Common.LogError(ex, false, true, "CheckLocalizations");
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
                Common.LogError(ex, false, $"Failed to download {url}", true, "CheckLocalizations");
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
