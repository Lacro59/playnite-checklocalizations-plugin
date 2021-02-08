using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CheckLocalizations.Views;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations
{
    public class CheckLocalizationsSettings : ISettings
    {
        private readonly CheckLocalizations plugin;

        public bool EnableCheckVersion { get; set; } = true;
        public bool MenuInExtensions { get; set; } = true;

        public bool EnableTag { get; set; } = false;
        public List<GameLanguage> GameLanguages { get; set; } = new List<GameLanguage>();

        public bool UiStyleSteam { get; set; } = false;
        public bool UiStylePcGamingWiki { get; set; } = true;

        public bool EnableIntegrationButton { get; set; } = false;
        public bool EnableIntegrationButtonDetails { get; set; } = false;
        public bool EnableIntegrationButtonJustIcon { get; set; } = true;

        public bool EnableIntegrationInDescription { get; set; } = false;
        public bool IntegrationShowTitle { get; set; } = true;
        public bool IntegrationTopGameDetails { get; set; } = true;
        public bool EnableIntegrationInCustomTheme { get; set; } = false;

        public bool EnableIntegrationFS { get; set; } = false;


        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonIgnore` ignore attribute.
        [JsonIgnore]
        public bool OptionThatWontBeSaved { get; set; } = false;


        // Parameterless constructor must exist if you want to use LoadPluginSettings method.
        public CheckLocalizationsSettings()
        {
        }

        public CheckLocalizationsSettings(CheckLocalizations plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<CheckLocalizationsSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                EnableCheckVersion = savedSettings.EnableCheckVersion;
                MenuInExtensions = savedSettings.MenuInExtensions;

                EnableTag = savedSettings.EnableTag;
                GameLanguages = savedSettings.GameLanguages;

                UiStyleSteam = savedSettings.UiStyleSteam;
                UiStylePcGamingWiki = savedSettings.UiStylePcGamingWiki;

                EnableIntegrationButton = savedSettings.EnableIntegrationButton;
                EnableIntegrationButtonDetails = savedSettings.EnableIntegrationButtonDetails;

                EnableIntegrationInDescription = savedSettings.EnableIntegrationInDescription;
                IntegrationShowTitle = savedSettings.IntegrationShowTitle;
                IntegrationTopGameDetails = savedSettings.IntegrationTopGameDetails;
                EnableIntegrationInCustomTheme = savedSettings.EnableIntegrationInCustomTheme;


                EnableIntegrationFS = savedSettings.EnableIntegrationFS;
            }

            if (GameLanguages.Count == 0)
            {
                GameLanguages = new List<GameLanguage>()
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

            if (GameLanguages.Count == 16)
            {
                GameLanguages.AddRange(new List<GameLanguage>()
                {
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
                    new GameLanguage { DisplayName = "Polski", Name = "Polish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Português", Name = "Portuguese", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Română", Name = "Romanian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Slovenčina", Name = "Slovenian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Српски", Name = "Serbian", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Svenska", Name = "Swedish", IsTag = false, IsNative = false },
                    new GameLanguage { DisplayName = "Українська", Name = "Ukrainian", IsTag = false, IsNative = false }
                });
            }
        }


        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {

        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {

        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            plugin.SavePluginSettings(this);

            CheckLocalizations.checkLocalizationsUI.RemoveElements();
            var TaskIntegrationUI = Task.Run(() =>
            {
                var dispatcherOp = CheckLocalizations.checkLocalizationsUI.AddElements();
                dispatcherOp.Completed += (s, e) => { CheckLocalizations.checkLocalizationsUI.RefreshElements(LocalizationsDatabase.GameSelected); };
            });

            CheckLocalizations.PluginDatabase.PluginSettings = this;
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
