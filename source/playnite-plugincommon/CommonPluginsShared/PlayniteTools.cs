using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using CommonPlayniteShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonPlayniteShared.Manifests;
using CommonPlayniteShared.Common;
using System.Text.RegularExpressions;
using CommonPluginsShared.Extensions;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Win32;
using Playnite.SDK.Plugins;
using System.Windows;

namespace CommonPluginsShared
{
    public class PlayniteTools
    {
        private static ILogger Logger => LogManager.GetLogger();

        private static List<Emulator> ListEmulators { get; set; } = null;

        private static HashSet<string> disabledPlugins;
        private static HashSet<string> DisabledPlugins => disabledPlugins ?? (disabledPlugins = GetDisabledPlugins());


        #region External plugin
        public enum ExternalPlugin
        {
            None,
            BattleNetLibrary,
            GogLibrary,
            OriginLibrary,
            PSNLibrary,
            SteamLibrary,
            XboxLibrary,
            IndiegalaLibrary,
            AmazonGamesLibrary,
            BethesdaLibrary,
            EpicLibrary,
            HumbleLibrary,
            ItchioLibrary,
            RockstarLibrary,
            TwitchLibrary,
            OculusLibrary,
            RiotLibrary,
            UplayLibrary,

            SuccessStory,
            CheckDlc,

            EmuLibrary,
            LegendaryLibrary,
            GogOssLibrary,
            GameActivity,
            MetadataLocal,
            IsThereAnyDeal,
            CheckLocalizations
        }

        private static readonly Dictionary<Guid, ExternalPlugin> PluginsById = new Dictionary<Guid, ExternalPlugin>
        {
            { new Guid("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD"), ExternalPlugin.BattleNetLibrary },
            { new Guid("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E"), ExternalPlugin.GogLibrary },
            { new Guid("85DD7072-2F20-4E76-A007-41035E390724"), ExternalPlugin.OriginLibrary },
            { new Guid("E4AC81CB-1B1A-4EC9-8639-9A9633989A71"), ExternalPlugin.PSNLibrary },
            { new Guid("CB91DFC9-B977-43BF-8E70-55F46E410FAB"), ExternalPlugin.SteamLibrary },
            { new Guid("7E4FBB5E-2AE3-48D4-8BA0-6B30E7A4E287"), ExternalPlugin.XboxLibrary },
            { new Guid("F7DA6EB0-17D7-497C-92FD-347050914954"), ExternalPlugin.IndiegalaLibrary },
            { new Guid("402674CD-4AF6-4886-B6EC-0E695BFA0688"), ExternalPlugin.AmazonGamesLibrary },
            { new Guid("0E2E793E-E0DD-4447-835C-C44A1FD506EC"), ExternalPlugin.BethesdaLibrary },
            { new Guid("00000002-DBD1-46C6-B5D0-B1BA559D10E4"), ExternalPlugin.EpicLibrary },
            { new Guid("96E8C4BC-EC5C-4C8B-87E7-18EE5A690626"), ExternalPlugin.HumbleLibrary },
            { new Guid("00000001-EBB2-4EEC-ABCB-7C89937A42BB"), ExternalPlugin.ItchioLibrary },
            { new Guid("88409022-088A-4DE8-805A-FDBAC291F00A"), ExternalPlugin.RockstarLibrary },
            { new Guid("E2A7D494-C138-489D-BB3F-1D786BEEB675"), ExternalPlugin.TwitchLibrary },
            { new Guid("C2F038E5-8B92-4877-91F1-DA9094155FC5"), ExternalPlugin.UplayLibrary },
            { new Guid("77346DD6-B0CC-4F7D-80F0-C1D138CCAE58"), ExternalPlugin.OculusLibrary },
            { new Guid("317A5E2E-EAC1-48BC-ADB3-FB9E321AFD3F"), ExternalPlugin.RiotLibrary },
            { new Guid("41E49490-0583-4148-94D2-940C7C74F1D9"), ExternalPlugin.EmuLibrary },
            { new Guid("EAD65C3B-2F8F-4E37-B4E6-B3DE6BE540C6"), ExternalPlugin.LegendaryLibrary },
            { new Guid("03689811-3F33-4DFB-A121-2EE168FB9A5C"), ExternalPlugin.GogOssLibrary },

            { new Guid("CEBE6D32-8C46-4459-B993-5A5189D60788"), ExternalPlugin.SuccessStory },
            { new Guid("BF78D9AF-6E79-4C73-ACA6-C23A11A485AE"), ExternalPlugin.CheckDlc },
            { new Guid("AFBB1A0D-04A1-4D0C-9AFA-C6E42CA855B4"), ExternalPlugin.GameActivity },
            { new Guid("FFB390B2-758F-40AC-9B20-9BE08FD05A65"), ExternalPlugin.MetadataLocal },
            { new Guid("7D5CBEE9-3C86-4389-AC7B-9ABE3DA4C9CD"), ExternalPlugin.IsThereAnyDeal },
            { new Guid("7CE83CFE-7894-4AD9-957D-7249C0FB3E7D"), ExternalPlugin.CheckLocalizations }
        };

        public static ExternalPlugin GetPluginType(Guid PluginId)
        {
            _ = PluginsById.TryGetValue(PluginId, out ExternalPlugin PluginSource);
            return PluginSource;
        }

        public static Guid GetPluginId(ExternalPlugin externalPlugin)
        {
            return PluginsById.FirstOrDefault(x => x.Value == externalPlugin).Key;
        }

        [Obsolete]
        public static bool IsDisabledPlaynitePlugins(string PluginName)
        {
            return DisabledPlugins?.Contains(PluginName) ?? false;
        }

        private static HashSet<string> GetDisabledPlugins()
        {
            try
            {
                string FileConfig = PlaynitePaths.ConfigFilePath;
                if (File.Exists(FileConfig))
                {
                    dynamic playniteConfig = Serialization.FromJsonFile<dynamic>(FileConfig);
                    dynamic disabledPlugins = playniteConfig.DisabledPlugins;
                    HashSet<string> output = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (disabledPlugins != null)
                    {
                        foreach (dynamic pluginName in disabledPlugins)
                        {
                            _ = output.Add(pluginName.ToString());
                        }
                    }
                    return output;
                }
                else
                {
                    Logger.Warn($"File not found {FileConfig}");
                    return new HashSet<string>();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }

        public static bool IsEnabledPlaynitePlugin(Guid id)
        {
            try
            {
                Plugin Plugin = API.Instance?.Addons?.Plugins?.FirstOrDefault(p => p.Id == id) ?? null;
                return Plugin != null;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return false;
        }
        #endregion


        #region Emulators
        /// <summary>
        /// Get configured emulators list
        /// </summary>
        /// <returns></returns>
        public static List<Emulator> GetListEmulators()
        {
            if (ListEmulators == null)
            {
                ListEmulators = new List<Emulator>();
                foreach (Emulator item in API.Instance.Database.Emulators)
                {
                    ListEmulators.Add(item);
                }
            }

            return ListEmulators;
        }

        public static Emulator GetGameEmulator(Game game)
        {
            if (!game.GameActions.HasItems())
            {
                return null;
            }

            foreach (GameAction gameAction in game.GameActions)
            {
                if (gameAction.Type != GameActionType.Emulator)
                {
                    continue;
                }

                Emulator emulator = API.Instance.Database.Emulators[gameAction.EmulatorId];
                if (emulator != null)
                {
                    return emulator;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if the game used an emulator
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool IsGameEmulated(Guid Id)
        {
            Game game = API.Instance.Database.Games.Get(Id);
            return IsGameEmulated(game);
        }

        /// <summary>
        /// Check if the game used an emulator
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        // TODO can be better
        public static bool IsGameEmulated(Game game)
        {
            if (game?.GameActions == null)
            {
                return false;
            }

            List<Emulator> ListEmulators = GetListEmulators();
            return game.GameActions.Where(x => x.IsPlayAction && ListEmulators.Any(y => y.Id == x?.EmulatorId)).Count() > 0;
        }

        /// <summary>
        /// Check if a game used RPCS3 emulator
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool GameUseRpcs3(Game game)
        {
            if (game?.GameActions == null)
            {
                return false;
            }

            foreach (GameAction action in game.GameActions)
            {
                Emulator emulator = API.Instance.Database.Emulators?.FirstOrDefault(e => e.Id == action?.EmulatorId);

                if (emulator == null)
                {
                    Logger.Warn($"No emulator found for {game.Name}");
                    return false;
                }

                string BuiltInConfigId = string.Empty;
                if (emulator.BuiltInConfigId == null)
                {
                    //logger.Warn($"No BuiltInConfigId found for {emulator.Name}");
                }
                else
                {
                    BuiltInConfigId = emulator.BuiltInConfigId;
                }

                if (BuiltInConfigId.Contains("rpcs3", StringComparison.OrdinalIgnoreCase)
                    || emulator.Name.Contains("rpcs3", StringComparison.OrdinalIgnoreCase)
                    || (emulator.InstallDir == null ? false : emulator.InstallDir.Contains("rpcs3", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GameUseScummVM(Game game)
        {
            if (game?.GameActions == null)
            {
                return false;
            }

            foreach (GameAction action in game.GameActions)
            {
                Emulator emulator = API.Instance.Database.Emulators?.FirstOrDefault(e => e.Id == action?.EmulatorId);

                if (emulator == null)
                {
                    Logger.Warn($"No emulator found for {game.Name}");
                    return false;
                }

                string BuiltInConfigId = string.Empty;
                if (emulator.BuiltInConfigId == null)
                {
                    Logger.Warn($"No BuiltInConfigId found for {emulator.Name}");
                }
                else
                {
                    BuiltInConfigId = emulator.BuiltInConfigId;
                }

                if (BuiltInConfigId.Contains("ScummVM", StringComparison.OrdinalIgnoreCase)
                    || emulator.Name.Contains("ScummVM", StringComparison.OrdinalIgnoreCase)
                    || emulator.InstallDir.Contains("ScummVM", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GameUseRetroArch(Game game)
        {
            if (game?.GameActions == null)
            {
                return false;
            }

            foreach (GameAction action in game.GameActions)
            {
                Emulator emulator = API.Instance.Database.Emulators?.FirstOrDefault(e => e.Id == action?.EmulatorId);

                if (emulator == null)
                {
                    Logger.Warn($"No emulator found for {game.Name}");
                    return false;
                }

                string BuiltInConfigId = string.Empty;
                if (emulator.BuiltInConfigId == null)
                {
                    Logger.Warn($"No BuiltInConfigId found for {emulator.Name}");
                }
                else
                {
                    BuiltInConfigId = emulator.BuiltInConfigId;
                }

                if (BuiltInConfigId.Contains("RetroArch", StringComparison.OrdinalIgnoreCase)
                    || emulator.Name.Contains("RetroArch", StringComparison.OrdinalIgnoreCase)
                    || emulator.InstallDir.Contains("RetroArch", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion


        /// <summary>
        /// Get file from cache
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="PluginName"></param>
        /// <returns></returns>
        public static string GetCacheFile(string FileName, string PluginName, dynamic Options = null)
        {
            PluginName = PluginName.ToLower();
            FileName = CommonPlayniteShared.Common.Paths.GetSafePathName(FileName);

            try
            {
                if (!Directory.Exists(Path.Combine(PlaynitePaths.DataCachePath, PluginName)))
                {
                    _ = Directory.CreateDirectory(Path.Combine(PlaynitePaths.DataCachePath, PluginName));
                }

                string PathImageFileName = Path.Combine(PlaynitePaths.DataCachePath, PluginName, FileName);

                if (File.Exists(PathImageFileName))
                {
                    return PathImageFileName;
                }
                else
                {
                    if (!FileName.IsNullOrEmpty() && Options?.CachedFileIfMissing ?? false)
                    {
                        _ = Task.Run(() =>
                        {
                            Common.LogDebug(true, $"DownloadFileImage is missing - {FileName}");
                            Web.DownloadFileImage(FileName, Options.Url, PlaynitePaths.DataCachePath, PluginName).GetAwaiter().GetResult();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on GetCacheFile({FileName})");
            }

            return string.Empty;
        }


        #region Game informations
        /// <summary>
        /// Get normalized source name
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static string GetSourceName(Guid Id)
        {
            Game game = API.Instance.Database.Games.Get(Id);
            return game == null ? "Playnite" : GetSourceName(game);
        }

        /// <summary>
        /// Get normalized source name
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetSourceName(Game game)
        {
            string SourceName = "Playnite";
            try
            {
                if (game == null)
                {
                    return SourceName;
                }

                if (game.PluginId != null)
                {
                    SourceName = GetSourceByPluginId(game.PluginId);
                }

                if (!SourceName.IsNullOrEmpty())
                {
                    return SourceName;
                }

                if (IsGameEmulated(game))
                {
                    SourceName = "RetroAchievements";
                    if (GameUseRpcs3(game))
                    {
                        SourceName = "Rpcs3";
                    }
                }
                else if (API.Instance.Database.Sources.Get(game.SourceId)?.Name.IsEqual("Xbox Game Pass") ?? false)
                {
                    SourceName = "Xbox";
                }
                else if (game.SourceId != null && game.SourceId != default)
                {
                    SourceName = API.Instance.Database.Sources.Get(game.SourceId)?.Name;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on GetSourceName({game.Name})");
            }

            return !SourceName.IsNullOrEmpty() ? SourceName : "Playnite";

        }

        public static string GetSourceNameOrPlatformForEmulated(Game game)
        {
            string sourceName = GetSourceName(game);
            if (IsGameEmulated(game))
            {
                string platormName = game.Platforms?.FirstOrDefault().Name;
                sourceName = platormName.IsNullOrEmpty() ? sourceName : platormName;
            }
            return sourceName;
        }

        public static string GetSourceByPluginId(Guid pluginId)
        {
            _ = PluginsById.TryGetValue(pluginId, out ExternalPlugin PluginSource);
            switch (PluginSource)
            {
                case ExternalPlugin.AmazonGamesLibrary:
                    return "Amazon Games";
                case ExternalPlugin.BattleNetLibrary:
                    return "Battle.NET";
                case ExternalPlugin.BethesdaLibrary:
                    return "Bethesda";
                case ExternalPlugin.EpicLibrary:
                case ExternalPlugin.LegendaryLibrary:
                    return "Epic";
                case ExternalPlugin.GogLibrary:
                    return "GOG";
                case ExternalPlugin.HumbleLibrary:
                    return "Humble";
                case ExternalPlugin.ItchioLibrary:
                    return "itch.io";
                case ExternalPlugin.OriginLibrary:
                    return "EA app";
                case ExternalPlugin.SteamLibrary:
                    return "Steam";
                case ExternalPlugin.TwitchLibrary:
                    return "Twitch";
                case ExternalPlugin.UplayLibrary:
                    return "Ubisoft Connect";
                case ExternalPlugin.XboxLibrary:
                    return "Xbox";
                case ExternalPlugin.PSNLibrary:
                    return "Playstation";
                case ExternalPlugin.IndiegalaLibrary:
                    return "Indiegala";
                case ExternalPlugin.RockstarLibrary:
                    return "Rockstar";
                case ExternalPlugin.OculusLibrary:
                    return "Oculus";
                case ExternalPlugin.RiotLibrary:
                    return "Riot Games";

                case ExternalPlugin.EmuLibrary:
                    return "EmuLibrary";

                case ExternalPlugin.None:
                case ExternalPlugin.SuccessStory:
                case ExternalPlugin.CheckDlc:

                default:
                    return string.Empty;
            }
        }

        public static string GetSourceBySourceIdOrPlatformId(Guid sourceId, List<Guid> platformsIds)
        {
            string sourceName = "Playnite";

            if (sourceId != default)
            {
                try
                {
                    GameSource Source = API.Instance.Database.Sources.Get(sourceId);
                    if (Source == null)
                    {
                        Logger.Warn($"SourceName not found for {sourceId}");
                        return "Playnite";
                    }
                    return Source.Name;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"SourceId: {sourceId}");
                    return "Playnite";
                }
            }

            if (platformsIds == null)
            {
                Logger.Warn($"No PlatformsIds for {Serialization.ToJson(platformsIds)}");
                return sourceName;
            }

            foreach (Guid PlatformID in platformsIds)
            {
                if (PlatformID != default)
                {
                    Platform platform = API.Instance.Database.Platforms.Get(PlatformID);
                    if (platform != null)
                    {
                        switch (platform.Name.ToLower())
                        {
                            case "pc":
                            case "pc (windows)":
                            case "pc (mac)":
                            case "pc (linux)":
                                return "Playnite";

                            default:
                                return platform.Name;
                        }
                    }
                }
            }

            return sourceName;
        }


        /// <summary>
        /// Get platform icon if defined
        /// </summary>
        /// <param name="PlatformName"></param>
        /// <returns></returns>
        public static string GetPlatformIcon(string PlatformName)
        {
            Platform PlatformFound = API.Instance.Database.Platforms?.Where(x => x.Name.IsEqual(PlatformName)).FirstOrDefault();
            return !(PlatformFound?.Icon).IsNullOrEmpty() ? API.Instance.Database.GetFullFilePath(PlatformFound.Icon) : string.Empty;
        }

        private static Regex NonWordCharactersAndTrimmableWhitespace = new Regex(@"(?<start>^[\W_]+)|(?<end>[\W_]+$)|(?<middle>[\W_]+)", RegexOptions.Compiled);
        private static Regex EditionInGameName = new Regex(@"\b(goty|game of the year|standard|deluxe|definitive|ultimate|platinum|gold|extended|complete|special|anniversary|enhanced)( edition)?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Remove all non-letter and non-number characters from a string, remove diacritics, make lowercase. For use when comparing game titles.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="removeEditions">Remove "game of the year", "complete edition" and the like from the string too</param>
        /// <returns></returns>
        public static string NormalizeGameName(string name, bool removeEditions = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string newName = name;
            newName = newName.Replace(" (CD)", string.Empty);

            if (removeEditions)
            {
                newName = EditionInGameName.Replace(newName, string.Empty);
            }

            MatchEvaluator matchEvaluator = (Match match) =>
            {
                if (match.Groups["middle"].Success) //if the match group is the last one in the regex (non-word characters, including whitespace, in the middle of a string)
                {
                    return " "; //replace (multiple) non-word character(s) in the middle of the string with a space
                }
                else
                {
                    return string.Empty; //remove non-word characters (including white space) at the start and end of the string
                }
            };
            newName = NonWordCharactersAndTrimmableWhitespace.Replace(newName, matchEvaluator).RemoveDiacritics();

            return newName.ToLowerInvariant();
        }

        public static string RemoveGameEdition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string newName = name;
            newName = newName.Replace(" (CD)", string.Empty);
            newName = EditionInGameName.Replace(newName, string.Empty);

            return newName.ToLowerInvariant();
        }
        #endregion


        public static void SetThemeInformation()
        {
            string defaultThemeName = "Default";
            ThemeManifest defaultTheme = new ThemeManifest()
            {
                DirectoryName = defaultThemeName,
                DirectoryPath = Path.Combine(PlaynitePaths.ThemesProgramPath, ThemeManager.GetThemeRootDir(ApplicationMode.Desktop), defaultThemeName),
                Name = defaultThemeName
            };
            ThemeManager.SetDefaultTheme(defaultTheme);

            ThemeManifest customTheme = null;
            string theme = API.Instance.ApplicationSettings.DesktopTheme;
            if (theme != ThemeManager.DefaultTheme.Name)
            {
                customTheme = ThemeManager.GetAvailableThemes(ApplicationMode.Desktop).FirstOrDefault(a => a.Id == theme);
                if (customTheme == null)
                {
                    ThemeManager.SetCurrentTheme(defaultTheme);
                }
                else
                {
                    ThemeManager.SetCurrentTheme(customTheme);
                }
            }
        }


        /// <summary>
        /// Remplace Playnite & Windows variables
        /// </summary>
        /// <param name="game"></param>
        /// <param name="inputString"></param>
        /// <param name="fixSeparators"></param>
        /// <returns></returns>
        public static string StringExpandWithoutStore(Game game, string inputString, bool fixSeparators = false)
        {
            if (string.IsNullOrEmpty(inputString) || !inputString.Contains('{'))
            {
                return inputString;
            }

            string result = inputString;

            // Playnite variables
            if (game == null)
            {
                game = new Game();
            }
            result = API.Instance.ExpandGameVariables(game, inputString);


            // Dropbox
            if (result.Contains("{Dropbox"))
            {
                string DropboxInfoFile = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "..", "Local", "Dropbox", "info.json");
                if (File.Exists(DropboxInfoFile))
                {
                    dynamic DropboxInfo = Serialization.FromJsonFile<dynamic>(DropboxInfoFile);
                    result = result.Replace("{DropboxFolder}", ((dynamic)DropboxInfo["personal"]["path"]).Value);
                }
            }

            // OneDrive
            if (result.Contains("{OneDrive"))
            {
                result = result.Replace("{OneDriveFolder}", GetOneDriveInstallationPath());
            }

            //RetroArchScreenshotsDir
            if (result.Contains("{RetroArchScreenshotsDir"))
            {
                string RetroarchScreenshots = string.Empty;
                Emulator emulator = API.Instance.Database.Emulators.FirstOrDefault(x => x.Name.Contains("RetroArch", StringComparison.OrdinalIgnoreCase));
                if (emulator != null)
                {
                    string cfg = Path.Combine(emulator.InstallDir, "retroarch.cfg");
                    if (File.Exists(cfg))
                    {
                        string line = string.Empty;
                        string Name = string.Empty;
                        StreamReader file = new StreamReader(cfg);
                        while ((line = file.ReadLine()) != null)
                        {
                            if (line.Contains("screenshot_directory", StringComparison.OrdinalIgnoreCase))
                            {
                                RetroarchScreenshots = line.Replace("screenshot_directory = ", string.Empty)
                                                            .Replace("\"", string.Empty)
                                                            .Trim();

                                if (RetroarchScreenshots.StartsWith(":"))
                                {
                                    RetroarchScreenshots = RetroarchScreenshots.Replace(":", emulator.InstallDir);
                                }
                            }
                        }
                        file.Close();
                    }
                }

                result = result.Replace("{RetroArchScreenshotsDir}", RetroarchScreenshots);
            }


            // Windows variables
            result = result.Replace("{WinDir}", Environment.GetEnvironmentVariable("WinDir"));
            result = result.Replace("{AllUsersProfile}", Environment.GetEnvironmentVariable("AllUsersProfile"));
            result = result.Replace("{AppData}", Environment.GetEnvironmentVariable("AppData"));
            result = result.Replace("{HomePath}", Environment.GetEnvironmentVariable("HomePath"));
            result = result.Replace("{UserName}", Environment.GetEnvironmentVariable("UserName"));
            result = result.Replace("{ComputerName}", Environment.GetEnvironmentVariable("ComputerName"));
            result = result.Replace("{UserProfile}", Environment.GetEnvironmentVariable("UserProfile"));
            result = result.Replace("{HomeDrive}", Environment.GetEnvironmentVariable("HomeDrive"));
            result = result.Replace("{SystemDrive}", Environment.GetEnvironmentVariable("SystemDrive"));
            result = result.Replace("{SystemRoot}", Environment.GetEnvironmentVariable("SystemRoot"));
            result = result.Replace("{Public}", Environment.GetEnvironmentVariable("Public"));
            result = result.Replace("{ProgramFiles}", Environment.GetEnvironmentVariable("ProgramFiles"));
            result = result.Replace("{CommonProgramFiles}", Environment.GetEnvironmentVariable("CommonProgramFiles"));
            result = result.Replace("{CommonProgramFiles(x86)}", Environment.GetEnvironmentVariable("CommonProgramFiles(x86)"));
            result = result.Replace("{CommonProgramW6432}", Environment.GetEnvironmentVariable("CommonProgramW6432"));
            result = result.Replace("{ProgramFiles(x86)}", Environment.GetEnvironmentVariable("ProgramFiles(x86)"));


            return fixSeparators ? CommonPlayniteShared.Common.Paths.FixSeparators(result) : result;
        }

        public static string PathToRelativeWithoutStores(Game game, string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            string result = inputString;


            string DropboxFolder = StringExpandWithoutStore(game, "{DropboxFolder}");
            if (!DropboxFolder.IsNullOrEmpty())
            {
                result = result.Replace(DropboxFolder, "{DropboxFolder}");
            }

            string OneDriveFolder = StringExpandWithoutStore(game, "{OneDriveFolder}");
            if (!OneDriveFolder.IsNullOrEmpty())
            {
                result = result.Replace(OneDriveFolder, "{OneDriveFolder}");
            }

            string RetroArchScreenshotsDir = StringExpandWithoutStore(game, "{RetroArchScreenshotsDir}");
            if (!RetroArchScreenshotsDir.IsNullOrEmpty())
            {
                result = result.Replace(RetroArchScreenshotsDir, "{RetroArchScreenshotsDir}");
            }


            string AppData = StringExpandWithoutStore(game, "{AppData}");
            result = result.Replace(AppData, "{AppData}");

            string AllUsersProfile = StringExpandWithoutStore(game, "{AllUsersProfile}");
            result = result.Replace(AllUsersProfile, "{AllUsersProfile}");

            string CommonProgramFiles = StringExpandWithoutStore(game, "{CommonProgramFiles}");
            result = result.Replace(CommonProgramFiles, "{CommonProgramFiles}");

            string CommonProgramFiles_x86 = StringExpandWithoutStore(game, "{CommonProgramFiles(x86)}");
            result = result.Replace(CommonProgramFiles_x86, "{CommonProgramFiles(x86)}");

            string CommonProgramW6432 = StringExpandWithoutStore(game, "{CommonProgramW6432}");
            result = result.Replace(CommonProgramW6432, "{CommonProgramW6432}");

            string ProgramFiles = StringExpandWithoutStore(game, "{ProgramFiles}");
            result = result.Replace(ProgramFiles, "{ProgramFiles}");

            string ProgramFiles_x86 = StringExpandWithoutStore(game, "{ProgramFiles(x86)}");
            result = result.Replace(ProgramFiles_x86, "{ProgramFiles(x86)}");

            string Public = StringExpandWithoutStore(game, "{Public}");
            result = result.Replace(Public, "{Public}");

            string WinDir = StringExpandWithoutStore(game, "{WinDir}");
            result = result.Replace(WinDir, "{WinDir}");

            string UserProfile = StringExpandWithoutStore(game, "{UserProfile}");
            result = result.Replace(UserProfile, "{UserProfile}");

            string SystemRoot = StringExpandWithoutStore(game, "{SystemRoot}");
            result = result.Replace(SystemRoot, "{SystemRoot}");

            string HomePath = StringExpandWithoutStore(game, "{HomePath}");
            result = result.Replace(HomePath, "{HomePath}");

            string SystemDrive = StringExpandWithoutStore(game, "{SystemDrive}");
            result = result.Replace(SystemDrive, "{SystemDrive}");

            string HomeDrive = StringExpandWithoutStore(game, "{HomeDrive}");
            result = result.Replace(HomeDrive, "{HomeDrive}");

            string UserName = StringExpandWithoutStore(game, "{UserName}");
            result = result.Replace(UserName, "{UserName}");

            string ComputerName = StringExpandWithoutStore(game, "{ComputerName}");
            result = result.Replace(ComputerName, "{ComputerName}");

            return result;
        }

        private static string GetOneDriveInstallationPath()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\OneDrive"))
            {
                if (key?.GetValueNames().Contains("UserFolder") == true)
                {
                    return key.GetValue("UserFolder")?.ToString().Replace('/', '\\') ?? string.Empty;
                }
            }

            return string.Empty;
        }


        public static void CreateLogPackage(string PluginName)
        {
            MessageBoxResult response = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCCommonCreateLog"), PluginName, MessageBoxButton.YesNo);
            if (response == MessageBoxResult.Yes)
            {
                string path = Path.Combine(PlaynitePaths.DataCachePath, PluginName + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip");

                FileSystem.DeleteFile(path);
                using (FileStream zipToOpen = new FileStream(path, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (string logFile in Directory.GetFiles(PlaynitePaths.ConfigRootPath, "*.log", SearchOption.TopDirectoryOnly))
                        {
                            if (Path.GetFileName(logFile) == "cef.log" || Path.GetFileName(logFile) == "debug.log")
                            {
                                continue;
                            }
                            else
                            {
                                _ = archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                            }
                        }
                    }
                }

                _ = Process.Start(PlaynitePaths.DataCachePath);
            }
        }


        public static void ShowPluginSettings(ExternalPlugin externalPlugin)
        {
            try
            {
                Guid PluginId = GetPluginId(externalPlugin);
                Plugin plugin = API.Instance.Addons.Plugins.FirstOrDefault(x => x.Id == PluginId);
                if (plugin != null)
                {
                    _ = plugin.OpenSettingsView();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }
    }
}
