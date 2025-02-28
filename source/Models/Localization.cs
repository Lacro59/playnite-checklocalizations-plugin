using CheckLocalizations.Services;
using CommonPluginsShared.Extensions;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CheckLocalizations.Models
{
    public enum SupportStatus
    {
        Native,
        NoNative,
        Hackable,
        NotApplicable,
        Unknown
    }


    public class Localization : ObservableObject
    {
        private static ILogger Logger => LogManager.GetLogger();
        private static LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;

        public string Language { get; set; }
        public SupportStatus Ui { get; set; }
        public SupportStatus Audio { get; set; }
        public SupportStatus Sub { get; set; }
        public string Notes { get; set; }
        public bool IsManual { get; set; }

        [DontSerialize]
        public string UiIcon => GetImage(Ui);
        [DontSerialize]
        public string AudioIcon => GetImage(Audio);
        [DontSerialize]
        public string SubIcon => GetImage(Sub);

        [DontSerialize]
        public bool IsKnowFlag
        {
            get
            {
                switch (Language.ToLower())
                {
                    case "afrikaans":
                    case "albanian":
                    case "arabic":
                    case "azerbaijani":
                    case "basque":
                    case "belarusian":
                    case "brazilian portuguese":
                    case "bulgarian":
                    case "canadian french":
                    case "catalan":
                    case "chinese simplified":
                    case "chinese traditional":
                    case "croatian":
                    case "czech":
                    case "danish":
                    case "dutch":
                    case "english":
                    case "estonian":
                    case "finnish":
                    case "filipino":
                    case "french":
                    case "galician":
                    case "german":
                    case "greek":
                    case "hebrew":
                    case "hindi":
                    case "hungarian":
                    case "icelandic":
                    case "indonesian":
                    case "italian":
                    case "japanese":
                    case "kannada":
                    case "kazakh":
                    case "korean":
                    case "latin american spanish":
                    case "latvian":
                    case "lithuanian":
                    case "macedonian":
                    case "malay":
                    case "polish":
                    case "portuguese":
                    case "portuguese - portugal":
                    case "romanian":
                    case "russian":
                    case "serbian":
                    case "simplified chinese":
                    case "slovak":
                    case "slovenian":
                    case "spanish":
                    case "spanish - latin america":
                    case "swedish":
                    case "thai":
                    case "traditional chinese":
                    case "turkish":
                    case "ukrainian":
                    case "vietnamese":
                    case "welsh":
                        return true;
                    default:
                        return false;
                }
            }
        }

        [DontSerialize]
        public string FlagIconPath
        {
            get
            {
                string pathResourcesFlags = Path.Combine(PluginDatabase.Paths.PluginPath, "Resources", "Flags");
                string alpha2 = PluginDatabase.PluginSettings.Settings.GameLanguages.FirstOrDefault(x => x.Name.IsEqual(Language))?.Alpha2;
                if (alpha2.IsEqual("en"))
                {
                    alpha2 = "us";
                }
                string finalPath = Path.Combine(pathResourcesFlags, $"{alpha2?.ToUpper()}@3x.png");

                if (File.Exists(finalPath))
                {
                    return finalPath;
                }
                else
                {
                    Logger.Warn($"No flag find for {Language} - {alpha2}");
                    return Path.Combine(pathResourcesFlags, $"__.png");
                }
            }
        }

        [DontSerialize]
        public BitmapImage FlagIcon => BitmapExtensions.BitmapFromFile(FlagIconPath);

        [DontSerialize]
        public string DisplayName
        {
            get
            {
                var gameLanguage = CheckLocalizations.PluginDatabase.PluginSettings.Settings.GameLanguages
                    .FirstOrDefault(x => x.Name.Equals(Language, StringComparison.OrdinalIgnoreCase));

                return gameLanguage?.DisplayName ?? Language;
            }
        }


        [DontSerialize]
        public bool IsOkUi => Ui == SupportStatus.Native || Ui == SupportStatus.Hackable;
        [DontSerialize]
        public bool IsOkAudio => Audio == SupportStatus.Native || Audio == SupportStatus.Hackable;
        [DontSerialize]
        public bool IsOkSub => Sub == SupportStatus.Native || Sub == SupportStatus.Hackable;

        private string GetImage(SupportStatus supportStatus)
        {
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            switch (supportStatus)
            {
                case SupportStatus.Hackable:
                    return $"{pluginFolder}\\Resources\\hackable.png";
                case SupportStatus.Native:
                    return $"{pluginFolder}\\Resources\\native.png";
                case SupportStatus.NoNative:
                    return $"{pluginFolder}\\Resources\\nonative.png";
                case SupportStatus.NotApplicable:
                    return $"{pluginFolder}\\Resources\\notapplicable.png";
                case SupportStatus.Unknown:
                    return $"{pluginFolder}\\Resources\\unknown.png";
                default:
                    return string.Empty;
            }
        }


        [DontSerialize]
        public bool UiStylePcGamingWiki => CheckLocalizations.PluginDatabase.PluginSettings.Settings.UiStylePcGamingWiki;
        [DontSerialize]
        public bool UiStyleSteam => CheckLocalizations.PluginDatabase.PluginSettings.Settings.UiStyleSteam;
    }
}
