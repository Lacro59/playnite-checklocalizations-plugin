using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using PluginCommon;
using CheckLocalizations.Views;
using CheckLocalizations.Services;
using Playnite.SDK.Events;
using Newtonsoft.Json;
using System.Windows;
using CheckLocalizations.Models;
using CheckLocalizations.Views.Interfaces;

namespace CheckLocalizations
{
    public class CheckLocalizations : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private CheckLocalizationsSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("7ce83cfe-7894-4ad9-957d-7249c0fb3e7d");

        private Game GameSelected { get; set; }
        private readonly IntegrationUI ui = new IntegrationUI();
        private LocalizationsApi localizationsApi { get; set; }
        private List<GameLocalization> gameLocalizations { get; set; }

        public CheckLocalizations(IPlayniteAPI api) : base(api)
        {
            settings = new CheckLocalizationsSettings(this);

            // Get plugin's location 
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add plugin localization in application ressource.
            PluginCommon.Localization.SetPluginLanguage(pluginFolder, api.Paths.ConfigurationPath);
            // Add common in application ressource.
            PluginCommon.Common.Load(pluginFolder);

            // Check version
            if (settings.EnableCheckVersion)
            {
                CheckVersion cv = new CheckVersion();

                if (cv.Check("CheckLocalizations", pluginFolder))
                {
                    cv.ShowNotification(api, "CheckLocalizations - " + resources.GetString("LOCUpdaterWindowTitle"));
                }
            }

            localizationsApi = new LocalizationsApi(this.GetPluginUserDataPath(), api, settings);

            // Custom theme button
            if (settings.EnableIntegrationInCustomTheme)
            {
                EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnCustomThemeButtonClick));
            }
        }

        public override IEnumerable<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>
            {
                new ExtensionFunction(
                    resources.GetString("LOCCheckLocalizations"),
                    () =>
                    {
                        // Add code to be execute when user invokes this menu entry.
                        new CheckLocalizationsView(gameLocalizations).ShowDialog();
                    })
            };
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            try
            {
                if (args.NewValue != null && args.NewValue.Count == 1)
                {
                    GameSelected = args.NewValue[0];

                    Integration();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", $"OnGameSelected()");
            }
        }

        private void OnCustomThemeButtonClick(object sender, RoutedEventArgs e)
        {
            string ButtonName = "";
            try
            {
                ButtonName = ((Button)sender).Name;
                if (ButtonName == "PART_ClCustomButton")
                {
                    OnBtGameSelectedActionBarClick(sender, e);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", "OnCustomThemeButtonClick() error");
            }
        }

        private void OnBtGameSelectedActionBarClick(object sender, RoutedEventArgs e)
        {
            new CheckLocalizationsView(gameLocalizations).ShowDialog();
        }


        private void Integration()
        {
            try
            {
                // Delete
                logger.Info("CheckLocalizations - Delete");
                ui.RemoveButtonInGameSelectedActionBarButtonOrToggleButton("PART_ClButton");
                ui.ClearElementInCustomTheme("PART_ClButtonCustom");
                ui.ClearElementInCustomTheme("PART_ClButtonCustomAdvanced");

                // Reset resources
                List<ResourcesList> resourcesLists = new List<ResourcesList>();
                resourcesLists.Add(new ResourcesList { Key = "Cl_HasData", Value = false });
                resourcesLists.Add(new ResourcesList { Key = "Cl_HasNativeSupport", Value = false });
                resourcesLists.Add(new ResourcesList { Key = "Cl_ListNativeSupport", Value = new List<GameLocalization>() });
                ui.AddResources(resourcesLists);


                List<Guid> ListEmulators = new List<Guid>();
                foreach (var item in PlayniteApi.Database.Emulators)
                {
                    ListEmulators.Add(item.Id);
                }
                if (GameSelected.PlayAction != null && GameSelected.PlayAction.EmulatorId != null && ListEmulators.Contains(GameSelected.PlayAction.EmulatorId))
                {
                    // Emulator
                }
                else
                {
                    gameLocalizations = new List<GameLocalization>();
                    var taskSystem = Task.Run(() =>
                    {
                        gameLocalizations = localizationsApi.GetLocalizations(GameSelected);

                        if (gameLocalizations.Count > 0)
                        {
                            resourcesLists.Add(new ResourcesList { Key = "Cl_HasData", Value = true });
                            
                            foreach (GameLanguage gameLanguage in settings.GameLanguages)
                            { 
                                if (gameLanguage.IsNative)
                                {
                                    if (gameLocalizations.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) != null)
                                    {
                                        resourcesLists.Add(new ResourcesList { Key = "Cl_HasNativeSupport", Value = true });
                                    }
                                }
                            }
                            resourcesLists.Add(new ResourcesList { Key = "Cl_ListNativeSupport", Value = gameLocalizations });
                        }
                    })
                    .ContinueWith(antecedent =>
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => {

                            ui.AddResources(resourcesLists);

                            // Auto integration
                            Button bt = new Button();
                            if (settings.EnableIntegrationButtonDetails)
                            {
     
                                bt = new ClButtonAdvanced((bool)resources.GetResource("Cl_HasNativeSupport"), gameLocalizations);
                                bt.Name = "PART_ClButton";
                                bt.Margin = new Thickness(10, 0, 0, 0);
                                ui.AddButtonInGameSelectedActionBarButtonOrToggleButton(bt);
                            }

                            if (settings.EnableIntegrationButton)
                            {
                                bt.Name = "PART_ClButton";
                                bt.Margin = new Thickness(10, 0, 0, 0);
                                bt.Content = resources.GetString("LOCCheckLocalizationsBtAdvancedSupportLanguages");
                                bt.Click += OnBtGameSelectedActionBarClick;
                                ui.AddButtonInGameSelectedActionBarButtonOrToggleButton(bt);
                            }


                            // Custom theme
                            if (settings.EnableIntegrationInCustomTheme)
                            {
                                // Create 
                                Button btCustom = new Button();
                                btCustom.Name = "PART_ClButtonCustom";
                                btCustom.Content = resources.GetString("LOCCheckLocalizationsBtAdvancedSupportLanguages");
                                btCustom.Click += OnBtGameSelectedActionBarClick;

                                ClButtonAdvanced btCustomAdvanced = new ClButtonAdvanced((bool)resources.GetResource("Cl_HasNativeSupport"), gameLocalizations);
                                btCustomAdvanced.Name = "PART_ClButtonCustomAdvanced";
                               
                                ui.AddElementInCustomTheme(btCustom, "PART_ClButtonCustom");
                                ui.AddElementInCustomTheme(btCustomAdvanced, "PART_ClButtonCustomAdvanced");
                            }
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", $"Impossible integration");
            }
        }




        public override void OnGameInstalled(Game game)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(Game game)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(Game game)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(Game game, long elapsedSeconds)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(Game game)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted()
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped()
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated()
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CheckLocalizationsSettingsView(this.GetPluginUserDataPath(), PlayniteApi, settings);
        }
    }
}