using CheckLocalizations.Services;
using CommonPluginsShared;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
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
        private static readonly ILogger logger = LogManager.GetLogger();
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

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
                    case "english":
                    case "french":
                    case "german":
                    case "italian":
                    case "japanese":
                    case "spanish":
                    case "simplified chinese":
                    case "russian":
                    case "traditional chinese":
                    case "korean":
                    case "polish":
                    case "brazilian portuguese":
                    case "arabic":
                    case "czech":
                    case "hungarian":
                    case "turkish":
                    case "catalan":
                    case "danish":
                    case "greek":
                    case "estonian":
                    case "persian":
                    case "finnish":
                    case "croatian":
                    case "indonesian":
                    case "lithuanian":
                    case "dutch":
                    case "norwegian":
                    case "portuguese":
                    case "portuguese - portugal":
                    case "romanian":
                    case "slovenian":
                    case "serbian":
                    case "swedish":
                    case "ukrainian":
                    case "latin american spanish":
                    case "spanish - latin america":
                    case "thai":
                    case "vietnamese":
                    case "hebrew":
                        return true;
                    default:
                        return false;
                }
            }
        }

        [DontSerialize]
        public BitmapImage FlagIcon
        {
            get
            {
                string PathResourcesFlags = Path.Combine(PluginDatabase.Paths.PluginPath, "Resources", "Flags");

                switch (Language.ToLower())
                {
                    case "english":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "en.png"));
                    case "french":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fr.png"));
                    case "german":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "de.png"));
                    case "italian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "it.png"));
                    case "japanese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ja.png"));
                    case "spanish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es-ES.png"));
                    case "simplified chinese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-CN.png"));
                    case "russian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ru.png"));
                    case "traditional chinese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-TW.png"));
                    case "korean":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ko.png"));
                    case "polish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pl.png"));
                    case "brazilian portuguese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pt-BR.png"));
                    case "arabic":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "af.png"));
                    case "czech":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "cs.png"));
                    case "hungarian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "hu.png"));
                    case "turkish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "tr.png"));
                    case "catalan":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ca.png"));
                    case "danish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "da.png"));
                    case "greek":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "el.png"));
                    case "estonian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "et.png"));
                    case "persian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fa.png"));
                    case "finnish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fi.png"));
                    case "croatian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "hr.png"));
                    case "indonesian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "id.png"));
                    case "lithuanian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "lt.png"));
                    case "dutch":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "nl.png"));
                    case "norwegian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "no.png"));
                    case "portuguese":
                    case "portuguese - portugal":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pt-PT.png"));
                    case "romanian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ro.png"));
                    case "slovenian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sk.png"));
                    case "serbian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sr.png"));
                    case "swedish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sv-SE.png"));
                    case "ukrainian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "uk.png"));
                    case "latin american spanish":
                    case "spanish - latin america":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es.png"));
                    case "thai":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "th.png"));
                    case "vietnamese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "vi.png"));
                    case "hebrew":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "he.png"));
                    default:
                        logger.Warn($"No flag find for {Language}");
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "__.png"));
                }
            }
        }

        [DontSerialize]
        public string DisplayName
        {
            get
            {
                GameLanguage gameLanguage = CheckLocalizations.PluginDatabase.PluginSettings.Settings.GameLanguages
                    .Find(x => x.Name.ToLower() == Language.ToLower());

                return gameLanguage == null ? Language : gameLanguage.DisplayName;
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
