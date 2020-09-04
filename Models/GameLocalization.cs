using Newtonsoft.Json;
using System;
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

    public class GameLocalization
    {
        public string Language { get; set; }
        public SupportStatus Ui { get; set; }
        public SupportStatus Audio { get; set; }
        public SupportStatus Sub { get; set; }
        public string Notes { get; set; }


        [JsonIgnore]
        public BitmapImage UiIcon
        {
            get
            {
                return GetImage(Ui);
            }
        }
        [JsonIgnore]
        public BitmapImage AudioIcon
        {
            get
            {
                return GetImage(Audio);
            }
        }
        [JsonIgnore]
        public BitmapImage SubIcon
        {
            get
            {
                return GetImage(Sub);
            }
        }


        private BitmapImage GetImage(SupportStatus supportStatus)
        {
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            BitmapImage iconImage = new BitmapImage();
            iconImage.BeginInit();
            switch (supportStatus)
            {
                case SupportStatus.Hackable:
                    iconImage.UriSource = new Uri($"{pluginFolder}\\Resources\\hackable.png", UriKind.RelativeOrAbsolute);
                    break;
                case SupportStatus.Native:
                    iconImage.UriSource = new Uri($"{pluginFolder}\\Resources\\native.png", UriKind.RelativeOrAbsolute);
                    break;
                case SupportStatus.NoNative:
                    iconImage.UriSource = new Uri($"{pluginFolder}\\Resources\\nonative.png", UriKind.RelativeOrAbsolute);
                    break;
                case SupportStatus.NotApplicable:
                    iconImage.UriSource = new Uri($"{pluginFolder}\\Resources\\notapplicable.png", UriKind.RelativeOrAbsolute);
                    break;
                case SupportStatus.Unknown:
                    iconImage.UriSource = new Uri($"{pluginFolder}\\Resources\\unknown.png", UriKind.RelativeOrAbsolute);
                    break;
            }
            iconImage.EndInit();
            return iconImage;
        }
    }
}
