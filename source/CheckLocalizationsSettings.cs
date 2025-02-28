﻿using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared.Plugins;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations
{
    public class CheckLocalizationsSettings : PluginSettings
    {
        #region Settings variables
        public bool EnableTagSingle { get; set; } = false;
        public bool EnableTagAudio { get; set; } = false;
        public List<GameLanguage> GameLanguages { get; set; } = new List<GameLanguage>();


        public bool UiStyleSteam { get; set; } = false;
        public bool UiStylePcGamingWiki { get; set; } = true;


        private bool _enableIntegrationViewItem = true;
        public bool EnableIntegrationViewItem { get => _enableIntegrationViewItem; set => SetValue(ref _enableIntegrationViewItem, value); }

        private bool _enableIntegrationButton = true;
        public bool EnableIntegrationButton { get => _enableIntegrationButton; set => SetValue(ref _enableIntegrationButton, value); }

        private bool _enableIntegrationButtonDetails = true;
        public bool EnableIntegrationButtonDetails { get => _enableIntegrationButtonDetails; set => SetValue(ref _enableIntegrationButtonDetails, value); }

        public bool EnableIntegrationButtonContextMenu { get; set; } = false;


        private bool _enableIntegrationListLanguages = true;
        public bool EnableIntegrationListLanguages { get => _enableIntegrationListLanguages; set => SetValue(ref _enableIntegrationListLanguages, value); }


        private bool _enableIntegrationFlags = true;
        public bool EnableIntegrationFlags { get => _enableIntegrationFlags; set => SetValue(ref _enableIntegrationFlags, value); }

        public bool OnlyDisplaySelectedFlags { get; set; } = false;
        public bool OnlyDisplayExistingFlags { get; set; } = false;


        public double ListLanguagesHeight { get; set; } = 120;
        public bool ListLanguagesWithColNote { get; set; } = false;
        public bool ListLanguagesVisibleEmpty { get; set; } = false;
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed

        private bool _hasNativeSupport = false;
        [DontSerialize]
        public bool HasNativeSupport { get => _hasNativeSupport; set => SetValue(ref _hasNativeSupport, value); }

        private List<Models.Localization> _listNativeSupport = new List<Models.Localization>();
        [DontSerialize]
        public List<Models.Localization> ListNativeSupport { get => _listNativeSupport; set => SetValue(ref _listNativeSupport, value); }
        #endregion  
    }


    public class CheckLocalizationsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CheckLocalizations Plugin;
        private CheckLocalizationsSettings EditingClone { get; set; }

        private CheckLocalizationsSettings _settings;
        public CheckLocalizationsSettings Settings { get => _settings; set => SetValue(ref _settings, value); }


        public CheckLocalizationsSettingsViewModel(CheckLocalizations plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            CheckLocalizationsSettings savedSettings = plugin.LoadPluginSettings<CheckLocalizationsSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            List<GameLanguage> gameLanguages = new List<GameLanguage>()
            {
                new GameLanguage { DisplayName = "العربية", Name = "Arabic", SteamCode = "ar", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "български", Name = "Bulgarian", SteamCode = "bg", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Català", Name = "Catalan", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Hrvatski", Name = "Croatian", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Čeština", Name = "Czech", SteamCode = "cs", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Dansk", Name = "Danish", SteamCode = "da", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Deutsch", Name = "German", SteamCode = "de", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Ελληνικά", Name = "Greek", SteamCode = "el", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "English", Name = "English", SteamCode = "en", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Español", Name = "Spanish", SteamCode = "es", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Español (Latinoamérica)", Name = "Latam", SteamCode = "es-419", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Eesti", Name = "Estonian", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "فارسی", Name = "Persian", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Suomi", Name = "Finnish", SteamCode = "fi", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Français", Name = "French", SteamCode = "fr", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Magyar", Name = "Hungarian", SteamCode = "hu", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Bahasa Indonesia", Name = "Indonesian", SteamCode = "id", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Italiano", Name = "Italian", SteamCode = "it", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "日本語", Name = "Japanese", SteamCode = "ja", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "한국어", Name = "Korean", SteamCode = "ko", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Lietuvių", Name = "Lithuanian", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Nederlands", Name = "Dutch", SteamCode = "nl", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Norsk", Name = "Norwegian", SteamCode = "no", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Polski", Name = "Polish", SteamCode = "pl", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Português", Name = "Portuguese", SteamCode = "pt", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Português Brasileiro", Name = "Brazilian Portuguese", SteamCode = "pt-BR", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Română", Name = "Romanian", SteamCode = "ro", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Русский", Name = "Russian", SteamCode = "ru", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Slovenčina", Name = "Slovenian", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Српски", Name = "Serbian", SteamCode = "", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Svenska", Name = "Swedish", SteamCode = "sv", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "ไทย", Name = "Thai", SteamCode = "th", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Türkçe", Name = "Turkish", SteamCode = "tr", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Українська", Name = "Ukrainian", SteamCode = "uk", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "Tiếng Việt", Name = "Vietnamese", SteamCode = "vi", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "简体中文", Name = "Simplified Chinese", SteamCode = "zh-CN", IsTag = false, IsNative = false },
                new GameLanguage { DisplayName = "繁體中文", Name = "Traditional Chinese", SteamCode = "zh-TW", IsTag = false, IsNative = false }
            };

            Settings = savedSettings ?? new CheckLocalizationsSettings { GameLanguages = gameLanguages };

            List<GameLanguage> missingLanguages = gameLanguages
                .Where(fl => !Settings.GameLanguages.Any(gl => gl.Name == fl.Name))
                .ToList();
            Settings.GameLanguages.AddRange(missingLanguages);
            foreach(GameLanguage gameLanguage in Settings.GameLanguages)
            {
                gameLanguage.SteamCode = gameLanguages.FirstOrDefault(x => x.Name == gameLanguage.Name)?.SteamCode ?? string.Empty;
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
            Settings.EnableTag = Settings.EnableTagAudio || Settings.EnableTagSingle;

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
