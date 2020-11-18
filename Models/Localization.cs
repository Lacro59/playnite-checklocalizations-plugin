using Newtonsoft.Json;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
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
                var gameLanguage = CheckLocalizations.GameLanguages.Find(x => x.Name.ToLower() == Language.ToLower());

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
    }
}
