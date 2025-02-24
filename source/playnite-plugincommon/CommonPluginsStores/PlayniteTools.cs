using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonPlayniteShared.Common;
using CommonPluginsStores.Steam;
using static CommonPluginsShared.PlayniteTools;

namespace CommonPluginsStores
{
    public class PlayniteTools
    {
        private static string SteamId = "null";
        private static string SteamAccountId = "null";
        private static string SteamInstallDir = "null";
        private static string SteamScreenshotsDir = "null";

        private static string UbisoftInstallDir = "null";
        private static string UbisoftScreenshotsDir = "null";



        public static List<string> ListVariables = new List<string>
        {
            "{InstallDir}", "{InstallDirName}", "{ImagePath}", "{ImageName}", "{ImageNameNoExt}", "{PlayniteDir}", "{Name}",
            "{Platform}", "{GameId}", "{DatabaseId}", "{PluginId}", "{Version}", "{EmulatorDir}",

            "{DropboxFolder}", "{OneDriveFolder}",

            "{SteamId}", "{SteamAccountId}", "{SteamInstallDir}", "{SteamScreenshotsDir}",
            "{UbisoftInstallDir}", "{UbisoftScreenshotsDir}",
            "{RetroArchScreenshotsDir}",

            "{WinDir}", "{AllUsersProfile}", "{AppData}", "{HomePath}", "{UserName}", "{ComputerName}", "{UserProfile}",
            "{HomeDrive}", "{SystemDrive}", "{SystemRoot}", "{Public}", "{CommonProgramW6432}", "{CommonProgramFiles}",
            "{ProgramFiles}", "{CommonProgramFiles(x86)}", "{ProgramFiles(x86)}"
        };

        public static string StringExpandWithStores(Game game, string inputString, bool fixSeparators = false)
        {
            if (string.IsNullOrEmpty(inputString) || !inputString.Contains('{'))
            {
                return inputString;
            }

            string result = inputString;
            result = StringExpandWithoutStore(game, result, fixSeparators);

            // Steam
            if (result.Contains("{Steam"))
            {
                SteamApi steamApi = new SteamApi("PlayniteTools", ExternalPlugin.None);

                if (SteamId == "null")
                {
                    SteamId = steamApi.CurrentAccountInfos?.UserId.ToString() ?? string.Empty;
                }
                if (SteamAccountId == "null")
                {
                    if (steamApi.CurrentAccountInfos != null)
                    {
                        SteamAccountId = SteamApi.GetAccountId(ulong.Parse(steamApi.CurrentAccountInfos.UserId)).ToString() ?? string.Empty;
                    }
                }
                if (SteamInstallDir == "null")
                {
                    SteamInstallDir = steamApi.GetInstallationPath();
                }
                if (SteamScreenshotsDir == "null")
                {
                    SteamScreenshotsDir = steamApi.GetScreeshotsPath();
                }

                result = SteamId.IsNullOrEmpty() ? result : result.Replace("{SteamId}", SteamId);
                result = SteamInstallDir.IsNullOrEmpty() ? result : result.Replace("{SteamInstallDir}", SteamInstallDir);
                result = SteamScreenshotsDir.IsNullOrEmpty() ? result : result.Replace("{SteamScreenshotsDir}", SteamScreenshotsDir);
            }

            // Ubisoft Connect
            if (result.Contains("{Ubisoft"))
            {
                UbisoftAPI ubisoftAPI = null;

                if (UbisoftInstallDir == "null")
                {
                    ubisoftAPI = ubisoftAPI ?? new UbisoftAPI();
                    UbisoftInstallDir = ubisoftAPI.GetInstallationPath();
                }
                if (UbisoftScreenshotsDir == "null")
                {
                    ubisoftAPI = ubisoftAPI ?? new UbisoftAPI();
                    UbisoftScreenshotsDir = ubisoftAPI.GetScreeshotsPath();
                }

                result = UbisoftInstallDir.IsNullOrEmpty() ? result : result.Replace("{UbisoftInstallDir}", UbisoftInstallDir);
                result = UbisoftScreenshotsDir.IsNullOrEmpty() ? result : result.Replace("{UbisoftScreenshotsDir}", UbisoftScreenshotsDir);
            }

            return fixSeparators ? Paths.FixSeparators(result) : result;
        }

        public static string PathToRelativeWithStores(Game game, string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            string result = inputString;

            string SteamId = StringExpandWithStores(game, "{SteamId}");
            result = SteamId.IsNullOrEmpty() ? result : result.Replace(SteamId, "{SteamId}");

            string SteamInstallDir = StringExpandWithStores(game, "{SteamInstallDir}");
            result = SteamInstallDir.IsNullOrEmpty() ? result : result.Replace(SteamInstallDir, "{SteamInstallDir}");

            string SteamScreenshotsDir = StringExpandWithStores(game, "{SteamScreenshotsDir}");
            result = SteamScreenshotsDir.IsNullOrEmpty() ? result : result.Replace(SteamScreenshotsDir, "{SteamScreenshotsDir}");

            string UbisoftInstallDir = StringExpandWithStores(game, "{UbisoftInstallDir}");
            result = UbisoftInstallDir.IsNullOrEmpty() ? result : result.Replace(UbisoftInstallDir, "{UbisoftInstallDir}");

            string UbisoftScreenshotsDir = StringExpandWithStores(game, "{UbisoftScreenshotsDir}");
            result = UbisoftScreenshotsDir.IsNullOrEmpty() ? result : result.Replace(UbisoftScreenshotsDir, "{UbisoftScreenshotsDir}");

            result = CommonPluginsShared.PlayniteTools.PathToRelativeWithoutStores(game, result);
            return result;
        }

        public static string NormalizeGameName(string name, bool removeEditions = false)
        {
            return CommonPluginsShared.PlayniteTools.NormalizeGameName(name, removeEditions);
        }
    }
}
