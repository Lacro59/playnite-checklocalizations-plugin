﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommonPlayniteShared.PluginLibrary.EpicLibrary.Models;//using EpicLibrary.Models;
using CommonPlayniteShared.Common;//using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Data;

namespace CommonPlayniteShared.PluginLibrary.EpicLibrary
{
    public class EpicLauncher
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        public const string GameLaunchUrlMask = @"com.epicgames.launcher://apps/{0}?action=launch&silent=true";
        public const string GameInstallUrlMask = @"com.epicgames.launcher://apps/{0}?action=install";
        public const string LibraryLaunchUrl = @"com.epicgames.launcher://store/library";

        public static string AllUsersPath => Path.Combine(Environment.ExpandEnvironmentVariables("%PROGRAMDATA%"), "Epic");

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : GetExecutablePath(path);
            }
        }

        public static string PortalConfigPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Launcher", "Portal", "Config", "DefaultPortalRegions.ini");
            }
        }

        public static string InstallationPath
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().
                    FirstOrDefault(a =>
                        a.DisplayName == "Epic Games Launcher" &&
                        !a.InstallLocation.IsNullOrEmpty() &&
                        File.Exists(GetExecutablePath(a.InstallLocation)));
                if (progs == null)
                {
                    // Try default location. These registry keys sometimes go missing on people's PCs...
                    if (File.Exists(GetExecutablePath(@"C:\Program Files (x86)\Epic Games\")))
                    {
                        return @"C:\Program Files (x86)\Epic Games\";
                    }
                    else if (File.Exists(GetExecutablePath(@"C:\Program Files\Epic Games\")))
                    {
                        return @"C:\Program Files\Epic Games\";
                    }

                    return string.Empty;
                }
                else
                {
                    return progs.InstallLocation;
                }
            }
        }

        public static bool IsInstalled
        {
            get
            {
                var path = InstallationPath;
                return !string.IsNullOrEmpty(path) && Directory.Exists(path);
            }
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\epicicon.png");

        public static void StartClient()
        {
            //ProcessStarter.StartProcess(ClientExecPath, string.Empty);
        }

        internal static string GetExecutablePath(string rootPath)
        {
            // Always prefer 32bit executable
            // https://github.com/JosefNemec/Playnite/issues/1552
            var p32 = Path.Combine(rootPath, "Launcher", "Portal", "Binaries", "Win32", "EpicGamesLauncher.exe");
            if (File.Exists(p32))
            {
                return p32;
            }
            else
            {
                return Path.Combine(rootPath, "Launcher", "Portal", "Binaries", "Win64", "EpicGamesLauncher.exe");
            }
        }

        public static List<LauncherInstalled.InstalledApp> GetInstalledAppList()
        {
            var installListPath = Path.Combine(AllUsersPath, "UnrealEngineLauncher", "LauncherInstalled.dat");
            if (!File.Exists(installListPath))
            {
                return new List<LauncherInstalled.InstalledApp>();
            }

            var list = Serialization.FromJson<LauncherInstalled>(FileSystem.ReadFileAsStringSafe(installListPath));
            return list.InstallationList;
        }

        public static List<InstalledManifiest> GetInstalledManifests()
        {
            var manifests = new List<InstalledManifiest>();
            var installListPath = Path.Combine(AllUsersPath, "EpicGamesLauncher", "Data", "Manifests");
            if (!Directory.Exists(installListPath))
            {
                return manifests;
            }

            foreach (var manFile in Directory.GetFiles(installListPath, "*.item"))
            {
                if (Serialization.TryFromJson<InstalledManifiest>(FileSystem.ReadFileAsStringSafe(manFile), out var manifest))
                {
                    // Some weird issue causes manifest to be created empty by Epic client
                    if (manifest != null)
                    {
                        manifests.Add(manifest);
                    }
                }
                else
                {
                    // This usually happens when user changes manifest manually (for example when moving games to a different drive)
                    // but they don't know what they are doing and resulting JSON is not a valid JSON anymore...
                    logger.Error("Failed to parse Epic installed game manifest: " + manFile);
                }
            }

            return manifests;
        }
    }
}
