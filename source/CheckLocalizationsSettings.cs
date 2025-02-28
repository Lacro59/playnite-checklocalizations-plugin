using CheckLocalizations.Models;
using CheckLocalizations.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Plugins;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public bool UpdateWhenHasManual { get; set; } = true;
        public bool AddedSimilarWhenManual { get; set; } = true;

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
            string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _ = Serialization.TryFromJsonFile(Path.Combine(pluginPath, "Data", "lang.json"), out List<GameLanguage> gameLanguages, out Exception ex);
            if (ex != null)
            {
                Common.LogError(ex, false, true, "CheckLocalization");
            }

            Settings = savedSettings ?? new CheckLocalizationsSettings { GameLanguages = gameLanguages };

            // TODO TEMP
            List<GameLanguage> missingLanguages = gameLanguages
                .Where(fl => !Settings.GameLanguages.Any(gl => gl.Name == fl.Name))
                .ToList();
            Settings.GameLanguages.AddRange(missingLanguages);
            foreach(GameLanguage gameLanguage in Settings.GameLanguages)
            {
                gameLanguage.Alpha2 = gameLanguages.FirstOrDefault(x => x.Name.IsEqual(gameLanguage.Name))?.Alpha2 ?? string.Empty;
                gameLanguage.Alpha3 = gameLanguages.FirstOrDefault(x => x.Name.IsEqual(gameLanguage.Name))?.Alpha3 ?? string.Empty;
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
