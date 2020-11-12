using CheckLocalizations.Models;
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

        public bool EnableTag { get; set; } = true;
        public List<GameLanguage> GameLanguages { get; set; }

        public bool EnableIntegrationButton { get; set; } = false;
        public bool EnableIntegrationButtonDetails { get; set; } = false;
        public bool EnableIntegrationButtonJustIcon { get; set; } = true;

        public bool EnableIntegrationInCustomTheme { get; set; } = false;

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

                EnableIntegrationButton = savedSettings.EnableIntegrationButton;
                EnableIntegrationButtonDetails = savedSettings.EnableIntegrationButtonDetails;

                EnableIntegrationInCustomTheme = savedSettings.EnableIntegrationInCustomTheme;
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
                };
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.

            if (CheckLocalizationsSettingsView.tokenSource != null)
            {
                CheckLocalizationsSettingsView.WithoutMessage = true;
                CheckLocalizationsSettingsView.tokenSource.Cancel();
            }
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(this);

            CheckLocalizations.checkLocalizationsUI.RemoveElements();
            var TaskIntegrationUI = Task.Run(() =>
            {
                CheckLocalizations.checkLocalizationsUI.AddElements();
                CheckLocalizations.checkLocalizationsUI.RefreshElements(CheckLocalizations.GameSelected);
            });
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}