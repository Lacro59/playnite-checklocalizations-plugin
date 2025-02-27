using CheckLocalizations.Services;
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
        public BitmapImage FlagIcon
        {
            get
            {
                string PathResourcesFlags = Path.Combine(PluginDatabase.Paths.PluginPath, "Resources", "Flags");

                switch (Language.ToLower())
                {
                    case "afrikaans":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "af.png"));
                    case "albanian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "al.png"));
                    case "arabic":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "arab.png"));
                    case "azerbaijani":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "az.png"));
                    case "basque":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es-PV.png"));
                    case "belarusian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "by.png"));
                    case "brazilian portuguese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pt-BR.png"));
                    case "bulgarian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "bg.png"));
                    case "canadian french":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fr-CA.png"));
                    case "catalan":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ca.png"));
                    case "chinese simplified":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-CN.png"));
                    case "chinese traditional":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-TW.png"));
                    case "croatian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "hr.png"));
                    case "czech":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "cs.png"));
                    case "danish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "da.png"));
                    case "dutch":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "nl.png"));
                    case "english":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "en.png"));
                    case "estonian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "et.png"));
                    case "finnish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fi.png"));
                    case "filipino":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ph.png"));
                    case "french":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fr.png"));
                    case "galician":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es-GA.png"));
                    case "german":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "de.png"));
                    case "greek":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "el.png"));
                    case "hebrew":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "he.png"));
                    case "hindi":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "in.png"));
                    case "hungarian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "hu.png"));
                    case "icelandic":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "is.png"));
                    case "indonesian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "id.png"));
                    case "italian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "it.png"));
                    case "japanese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ja.png"));
                    case "kannada":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "in.png"));
                    case "kazakh":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "kz.png"));
                    case "korean":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ko.png"));
                    case "latin american spanish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es.png"));
                    case "latvian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "lv.png"));
                    case "lithuanian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "lt.png"));
                    case "macedonian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "mk.png"));
                    case "malay":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "my.png"));
                    case "polish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pl.png"));
                    case "portuguese":
                    case "portuguese - portugal":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pt-PT.png"));
                    case "romanian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ro.png"));
                    case "russian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ru.png"));
                    case "serbian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sr.png"));
                    case "simplified chinese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-CN.png"));
                    case "slovak":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sk.png"));
                    case "slovenian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "si.png"));
                    case "spanish":
                    case "spanish - latin america":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es-ES.png"));
                    case "swedish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sv-SE.png"));
                    case "thai":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "th.png"));
                    case "traditional chinese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-TW.png"));
                    case "turkish":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "tr.png"));
                    case "ukrainian":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "uk.png"));
                    case "vietnamese":
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "vi.png"));
                    default:
                        Logger.Warn($"No flag find for {Language}");
                        return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "__.png"));
                }
            }
        }

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
