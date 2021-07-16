using CheckLocalizations.Services;
using Playnite.SDK.Data;
using System;
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
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;

        public string Language { get; set; }
        public SupportStatus Ui { get; set; }
        public SupportStatus Audio { get; set; }
        public SupportStatus Sub { get; set; }
        public string Notes { get; set; }
        public bool IsManual { get; set; }

        [DontSerialize]
        public string UiIcon
        {
            get
            {
                return GetImage(Ui);
            }
        }
        [DontSerialize]
        public string AudioIcon
        {
            get
            {
                return GetImage(Audio);
            }
        }
        [DontSerialize]
        public string SubIcon
        {
            get
            {
                return GetImage(Sub);
            }
        }

        [DontSerialize]
        public BitmapImage FlagIcon
        {
            get
            {
                string PathResourcesFlags = Path.Combine(PluginDatabase.Paths.PluginPath, "Resources", "Flags");

                if (Language == "English")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "en.png"));
                }

                if (Language == "French")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fr.png"));
                }

                if (Language == "German")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "de.png"));
                }

                if (Language == "Italian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "it.png"));
                }

                if (Language == "Japanese")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ja.png"));
                }

                if (Language == "Spanish")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "es-ES.png"));
                }

                if (Language == "Simplified Chinese")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-CN.png"));
                }

                if (Language == "Russian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ru.png"));
                }

                if (Language == "Traditional Chinese")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "zh-TW.png"));
                }

                if (Language == "Korean")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ko.png"));
                }

                if (Language == "Polish")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pl.png"));
                }

                if (Language == "Brazilian Portuguese")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pt-BR.png"));
                }

                if (Language == "Arabic")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "af.png"));
                }

                if (Language == "Czech")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "cs.png"));
                }

                if (Language == "Hungarian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "hu.png"));
                }

                if (Language == "Turkish")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "tr.png"));
                }

                if (Language == "Catalan")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ca.png"));
                }

                if (Language == "Danish")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "da.png"));
                }

                if (Language == "Greek")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "el.png"));
                }

                if (Language == "Estonian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "et.png"));
                }

                if (Language == "Persian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fa.png"));
                }

                if (Language == "Finnish")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "fi.png"));
                }

                if (Language == "Croatian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "hr.png"));
                }

                if (Language == "Indonesian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "id.png"));
                }

                if (Language == "Lithuanian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "lt.png"));
                }

                if (Language == "Dutch")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "nl.png"));
                }

                if (Language == "Norwegian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "no.png"));
                }

                if (Language == "Portuguese")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "pt-PT.png"));
                }

                if (Language == "Romanian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "ro.png"));
                }

                if (Language == "Slovenian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sk.png"));
                }

                if (Language == "Serbian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sr.png"));
                }

                if (Language == "Swedish")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "sv-SE.png"));
                }

                if (Language == "Ukrainian")
                {
                    return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "uk.png"));
                }

                return BitmapExtensions.BitmapFromFile(Path.Combine(PathResourcesFlags, "__.png"));
            }
        }

        [DontSerialize]
        public string DisplayName
        {
            get
            {
                var gameLanguage = CheckLocalizations.PluginDatabase.PluginSettings.Settings.GameLanguages.Find(x => x.Name.ToLower() == Language.ToLower());

                if (gameLanguage == null)
                {
                    return Language;
                }
                else
                {
                    return gameLanguage.DisplayName;
                }
            }
        }

        [DontSerialize]
        public bool IsOkUi
        {
            get
            {
                return (Ui == SupportStatus.Native || Ui == SupportStatus.Hackable);
            }
        }
        [DontSerialize]
        public bool IsOkAudio
        {
            get
            {
                return (Audio == SupportStatus.Native || Audio == SupportStatus.Hackable);
            }
        }
        [DontSerialize]
        public bool IsOkSub
        {
            get
            {
                return (Sub == SupportStatus.Native || Sub == SupportStatus.Hackable);
            }
        }

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
            }

            return string.Empty;
        }


        [DontSerialize]
        public bool UiStylePcGamingWiki
        {
            get
            {
                return CheckLocalizations.PluginDatabase.PluginSettings.Settings.UiStylePcGamingWiki;
            }
        }
        [DontSerialize]
        public bool UiStyleSteam
        {
            get
            {
                return CheckLocalizations.PluginDatabase.PluginSettings.Settings.UiStyleSteam;
            }
        }
    }
}
