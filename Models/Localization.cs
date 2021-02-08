using Newtonsoft.Json;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
        public string Language { get; set; }
        public SupportStatus Ui { get; set; }
        public SupportStatus Audio { get; set; }
        public SupportStatus Sub { get; set; }
        public string Notes { get; set; }
        public bool IsManual { get; set; }

        [JsonIgnore]
        public string UiIcon
        {
            get
            {
                return GetImage(Ui);
            }
        }
        [JsonIgnore]
        public string AudioIcon
        {
            get
            {
                return GetImage(Audio);
            }
        }
        [JsonIgnore]
        public string SubIcon
        {
            get
            {
                return GetImage(Sub);
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsOkUi
        {
            get
            {
                return (Ui == SupportStatus.Native || Ui == SupportStatus.Hackable);
            }
        }
        [JsonIgnore]
        public bool IsOkAudio
        {
            get
            {
                return (Audio == SupportStatus.Native || Audio == SupportStatus.Hackable);
            }
        }
        [JsonIgnore]
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


        [JsonIgnore]
        public bool UiStylePcGamingWiki
        {
            get
            {
                return CheckLocalizations.PluginDatabase.PluginSettings.Settings.UiStylePcGamingWiki;
            }
        }
        [JsonIgnore]
        public bool UiStyleSteam
        {
            get
            {
                return CheckLocalizations.PluginDatabase.PluginSettings.Settings.UiStyleSteam;
            }
        }
    }
}
