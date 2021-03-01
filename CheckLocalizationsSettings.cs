using CheckLocalizations.Models;
using CheckLocalizations.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations
{
    public class CheckLocalizationsSettings : ObservableObject
    {
        #region Settings variables
        public bool MenuInExtensions { get; set; } = true;

        public bool EnableTag { get; set; } = false;
        public bool EnableTagAuto { get; set; } = false;
        public List<GameLanguage> GameLanguages { get; set; } = new List<GameLanguage>();


        public bool UiStyleSteam { get; set; } = false;
        public bool UiStylePcGamingWiki { get; set; } = true;


        private bool _EnableIntegrationViewItem { get; set; } = false;
        public bool EnableIntegrationViewItem
        {
            get => _EnableIntegrationViewItem;
            set
            {
                _EnableIntegrationViewItem = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationButton { get; set; } = false;
        public bool EnableIntegrationButton
        {
            get => _EnableIntegrationButton;
            set
            {
                _EnableIntegrationButton = value;
                OnPropertyChanged();
            }
        }

        public bool EnableIntegrationButtonDetails { get; set; } = false;
        public bool EnableIntegrationButtonContextMenu { get; set; } = false;


        private bool _EnableIntegrationListLanguages { get; set; } = false;
        public bool EnableIntegrationListLanguages
        {
            get => _EnableIntegrationListLanguages;
            set
            {
                _EnableIntegrationListLanguages = value;
                OnPropertyChanged();
            }
        }

        public double ListLanguagesHeight { get; set; } = 120;
        public bool ListLanguagesWithColNote { get; set; } = false;
        public bool ListLanguagesVisibleEmpty { get; set; } = false;
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed
        [DontSerialize]
        private bool _HasData { get; set; } = false;
        public bool HasData
        {
            get => _HasData;
            set
            {
                _HasData = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private bool _HasNativeSupport { get; set; } = false;
        public bool HasNativeSupport
        {
            get => _HasNativeSupport;
            set
            {
                _HasNativeSupport = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        private List<Models.Localization> _ListNativeSupport { get; set; } = new List<Models.Localization>();
        public List<Models.Localization> ListNativeSupport
        {
            get => _ListNativeSupport;
            set
            {
                _ListNativeSupport = value;
                OnPropertyChanged();
            }
        }
        #endregion  
    }


    public class CheckLocalizationsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CheckLocalizations Plugin;
        private CheckLocalizationsSettings EditingClone { get; set; }

        private CheckLocalizationsSettings _Settings;
        public CheckLocalizationsSettings Settings
        {
            get => _Settings;
            set
            {
                _Settings = value;
                OnPropertyChanged();
            }
        }


        public CheckLocalizationsSettingsViewModel(CheckLocalizations plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<CheckLocalizationsSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new CheckLocalizationsSettings();                
                Settings.GameLanguages = new List<GameLanguage>()
                {
                    new GameLanguage { DisplayName = "English", Name = "English", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Français", Name = "French", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Deutsch", Name = "German", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Italiano", Name = "Italian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "日本語", Name = "Japanese", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Español", Name = "Spanish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "简体中文", Name = "Simplified Chinese", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Русский", Name = "Russian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "繁體中文", Name = "Traditional Chinese", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "한국어", Name = "Korean", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Polski", Name = "Polish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Português Brasileiro", Name = "Brazilian Portuguese", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "العربية", Name = "Arabic", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Čeština", Name = "Czech", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Magyar", Name = "Hungarian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Türkçe", Name = "Turkish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "عربى", Name = "Arabic", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Català", Name = "Catalan", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "dansk", Name = "Danish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Ελληνικά", Name = "Greek", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Eesti", Name = "Estonian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "فارسی", Name = "Persian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Suomi", Name = "Finnish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Hrvatski", Name = "Croatian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Magyar", Name = "Hungarian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Bahasa Indonesia", Name = "Indonesian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Lietuvių", Name = "Lithuanian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Nederlands", Name = "Dutch", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Norsk", Name = "Norwegian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Português", Name = "Portuguese", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Română", Name = "Romanian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Slovenčina", Name = "Slovenian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Српски", Name = "Serbian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Svenska", Name = "Swedish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Українська", Name = "Ukrainian", IsTag = false, IsNative = false }
                };
            }
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            Plugin.SavePluginSettings(Settings);
            CheckLocalizations.PluginDatabase.PluginSettings = this;
            this.OnPropertyChanged();
        }

        // Code execute when user decides to confirm changes made since BeginEdit was called.
        // Executed before EndEdit is called and EndEdit is not called if false is returned.
        // List of errors is presented to user if verification fails.
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
