using CheckLocalizations.Models;
using CheckLocalizations.Views;
using CheckLocalizations.Views.Interfaces;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CheckLocalizations.Services
{
    public class CheckLocalizationsUI : PlayniteUiHelper
    {
        private readonly CheckLocalizationsSettings _Settings;
        public override string _PluginUserDataPath { get; set; } = string.Empty;

        public override bool IsFirstLoad { get; set; } = true;

        public override string BtActionBarName { get; set; } = string.Empty;
        public override FrameworkElement PART_BtActionBar { get; set; }

        public override string SpDescriptionName { get; set; } = string.Empty;
        public override FrameworkElement PART_SpDescription { get; set; }

        public override List<CustomElement> ListCustomElements { get; set; } = new List<CustomElement>();

        
        public CheckLocalizationsUI(IPlayniteAPI PlayniteApi, CheckLocalizationsSettings Settings, string PluginUserDataPath) : base(PlayniteApi, PluginUserDataPath)
        {
            _Settings = Settings;
            _PluginUserDataPath = PluginUserDataPath;

            BtActionBarName = "PART_ClButton";
            SpDescriptionName = "PART_ClDescriptionIntegration";
        }


        public override void Initial()
        {
            CheckLocalizations.PluginDatabase.GameSelectedData = new GameLocalizations();

            if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (_Settings.EnableIntegrationButton)
                {
#if DEBUG
                    logger.Debug($"CheckLocalizations - InitialBtActionBar()");
#endif
                    InitialBtActionBar();
                }

                if (_Settings.EnableIntegrationInDescription)
                {
#if DEBUG
                    logger.Debug($"CheckLocalizations - InitialSpDescription()");
#endif
                    InitialSpDescription();
                }

                if (_Settings.EnableIntegrationInCustomTheme)
                {
#if DEBUG
                    logger.Debug($"CheckLocalizations - InitialCustomElements()");
#endif
                    InitialCustomElements();
                }
            }
        }

        public override DispatcherOperation AddElements()
        {
            if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                if (IsFirstLoad)
                {
#if DEBUG
                    logger.Debug($"CheckLocalizations - IsFirstLoad");
#endif
                    Thread.Sleep(2000);
                    IsFirstLoad = false;
                }

                return Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    CheckTypeView();

                    if (_Settings.EnableIntegrationButton)
                    {
#if DEBUG
                        logger.Debug($"CheckLocalizations - AddBtActionBar()");
#endif
                        AddBtActionBar();
                    }

                    if (_Settings.EnableIntegrationInDescription)
                    {
#if DEBUG
                        logger.Debug($"CheckLocalizations - AddSpDescription()");
#endif
                        AddSpDescription();
                    }

                    if (_Settings.EnableIntegrationInCustomTheme)
                    {
#if DEBUG
                        logger.Debug($"CheckLocalizations - AddCustomElements()");
#endif
                        AddCustomElements();
                    }
                }));
            }

            return null;
        }

        public override void RefreshElements(Game GameSelected, bool Force = false)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            Task TaskRefreshBtActionBar = Task.Run(() =>
            {
                try
                {
                    Initial();

                    // Reset resources
                    List<ResourcesList> resourcesLists = new List<ResourcesList>();
                    resourcesLists.Add(new ResourcesList { Key = "Cl_HasData", Value = false });
                    resourcesLists.Add(new ResourcesList { Key = "Cl_HasNativeSupport", Value = false });
                    resourcesLists.Add(new ResourcesList { Key = "Cl_ListNativeSupport", Value = new List<Models.Localization>() });
                    ui.AddResources(resourcesLists);

                    if (!PlayniteTools.IsGameEmulated(_PlayniteApi, GameSelected))
                    {
                        // Load data
                        GameLocalizations gameLocalizations = CheckLocalizations.PluginDatabase.Get(GameSelected);

                        if (gameLocalizations.Items.Count > 0)
                        {
                            resourcesLists.Add(new ResourcesList { Key = "Cl_HasData", Value = true });

                            foreach (GameLanguage gameLanguage in _Settings.GameLanguages)
                            {
                                if (gameLanguage.IsNative)
                                {
                                    if (gameLocalizations.Items.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) != null)
                                    {
                                        resourcesLists.Add(new ResourcesList { Key = "Cl_HasNativeSupport", Value = true });
                                    }
                                }
                            }
                            resourcesLists.Add(new ResourcesList { Key = "Cl_ListNativeSupport", Value = gameLocalizations });
                        }
                        else
                        {
                            logger.Info($"CheckLocalizations - No data find for {GameSelected.Name}");
                        }

#if DEBUG
                        logger.Debug($"CheckLocalizations - {GameSelected.Name} - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");
#endif
                        // If not cancel, show
                        if (!ct.IsCancellationRequested && GameSelected.Id == CheckLocalizations.GameSelected.Id)
                        {
                            ui.AddResources(resourcesLists);

                            if (_PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
                            {
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                                {
                                    CheckLocalizations.PluginDatabase.SetCurrent(gameLocalizations);

                                    if (_Settings.EnableIntegrationButton)
                                    {
#if DEBUG
                                    logger.Debug($"CheckLocalizations - RefreshBtActionBar()");
#endif
                                    RefreshBtActionBar();
                                    }

                                    if (_Settings.EnableIntegrationInDescription)
                                    {
#if DEBUG
                                    logger.Debug($"CheckLocalizations - RefreshSpDescription()");
#endif
                                    RefreshSpDescription();
                                    }

                                    if (_Settings.EnableIntegrationInCustomTheme)
                                    {
#if DEBUG
                                    logger.Debug($"CheckLocalizations - RefreshCustomElements()");
#endif
                                    RefreshCustomElements();
                                    }
                                }));
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"CheckLocalizations - No treatment for emulated game");
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", $"Error on TaskRefreshBtActionBar()");
                }
            }, ct);

            taskHelper.Add(TaskRefreshBtActionBar, tokenSource);
        }


        #region BtActionBar
        public override void InitialBtActionBar()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                if (PART_BtActionBar != null)
                {
                    PART_BtActionBar.Visibility = Visibility.Visible;
                }
            });
        }

        public override void AddBtActionBar()
        {
            if (PART_BtActionBar != null)
            {
#if DEBUG
                logger.Debug($"CheckLocalizations - PART_BtActionBar allready insert");
#endif
                return;
            }


            Button BtActionBar = new Button();

            if (_Settings.EnableIntegrationButtonDetails)
            {
                BtActionBar = new ClButtonAdvanced(_Settings.EnableIntegrationButtonJustIcon);
            }
            else
            {
                BtActionBar = new ClButton(_Settings.EnableIntegrationButtonJustIcon);
                BtActionBar.Click += OnBtActionBarClick;
            }

            BtActionBar.Name = BtActionBarName;
            BtActionBar.Margin = new Thickness(10, 0, 0, 0);

            try
            {
                ui.AddButtonInGameSelectedActionBarButtonOrToggleButton(BtActionBar);
                PART_BtActionBar = IntegrationUI.SearchElementByName(BtActionBarName);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", "Error on AddBtActionBar()");
            }
        }

        public override void RefreshBtActionBar()
        {
            if (PART_BtActionBar != null)
            {
                PART_BtActionBar.Visibility = Visibility.Visible;

                if (PART_BtActionBar is ClButtonAdvanced)
                {

                }
            }
            else
            {
                logger.Warn($"CheckLocalizations - PART_BtActionBar is not defined");
            }
        }


        public void OnBtActionBarClick(object sender, RoutedEventArgs e)
        {
            var ViewExtension = new CheckLocalizationsView();
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(_PlayniteApi, "CheckLocalizations", ViewExtension);
            windowExtension.ShowDialog();
        }

        public void OnCustomThemeButtonClick(object sender, RoutedEventArgs e)
        {
            if (_Settings.EnableIntegrationInCustomTheme)
            {
                string ButtonName = string.Empty;
                try
                {
                    ButtonName = ((Button)sender).Name;
                    if (ButtonName == "PART_ClCustomButton")
                    {
                        OnBtActionBarClick(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", "OnCustomThemeButtonClick() error");
                }
            }
        }
        #endregion


        #region SpDescription
        public override void InitialSpDescription()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                if (PART_SpDescription != null)
                {
                    PART_SpDescription.Visibility = Visibility.Visible;
                }
            });
        }

        public override void AddSpDescription()
        {
            if (PART_SpDescription != null)
            {
#if DEBUG
                logger.Debug($"CheckLocalizations - PART_SpDescription allready insert");
#endif
                return;
            }

            try
            {
                ClDescriptionIntegration SpDescription = new ClDescriptionIntegration(_Settings.IntegrationShowTitle, false);
                SpDescription.Name = SpDescriptionName;

                ui.AddElementInGameSelectedDescription(SpDescription, _Settings.IntegrationTopGameDetails);
                PART_SpDescription = IntegrationUI.SearchElementByName(SpDescriptionName);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", "Error on AddSpDescription()");
            }
        }

        public override void RefreshSpDescription()
        {
            if (PART_SpDescription != null)
            {
                PART_SpDescription.Visibility = Visibility.Visible;
            }
            else
            {
                logger.Warn($"CheckLocalizations - PART_SpDescription is not defined");
            }
        }
        #endregion  


        #region CustomElements
        public override void InitialCustomElements()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                foreach (CustomElement customElement in ListCustomElements)
                {
                    customElement.Element.Visibility = Visibility.Visible;
                }
            });
        }

        public override void AddCustomElements()
        {
            if (ListCustomElements.Count > 0)
            {
#if DEBUG
                logger.Debug($"CheckLocalizations - CustomElements allready insert - {ListCustomElements.Count}");
#endif
                return;
            }

            FrameworkElement PART_ClButtonWithJustIcon = null;
            FrameworkElement PART_ClButtonWithTitle = null;
            FrameworkElement PART_ClButtonWithTitleAndDetails = null;
            FrameworkElement PART_ClButtonWithJustIconAndDetails = null;

            FrameworkElement PART_ClListLanguages = null;
            try
            {
                PART_ClButtonWithJustIcon = IntegrationUI.SearchElementByName("PART_ClButtonWithJustIcon", false, true);
                PART_ClButtonWithTitle = IntegrationUI.SearchElementByName("PART_ClButtonWithTitle", false, true);
                PART_ClButtonWithTitleAndDetails = IntegrationUI.SearchElementByName("PART_ClButtonWithTitleAndDetails", false, true);
                PART_ClButtonWithJustIconAndDetails = IntegrationUI.SearchElementByName("PART_ClButtonWithJustIconAndDetails", false, true);

                PART_ClListLanguages = IntegrationUI.SearchElementByName("PART_ClListLanguages", false, true);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations", $"Error on find custom element");
            }

            if (PART_ClButtonWithJustIcon != null)
            {
                PART_ClButtonWithJustIcon = new ClButton(true);
                ((Button)PART_ClButtonWithJustIcon).Click += OnBtActionBarClick;
                try
                {
                    ui.AddElementInCustomTheme(PART_ClButtonWithJustIcon, "PART_ClButtonWithJustIcon");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ClButtonWithJustIcon", Element = PART_ClButtonWithJustIcon });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", "Error on AddCustomElements()");
                }
            }

            if (PART_ClButtonWithTitle != null)
            {
                PART_ClButtonWithTitle = new ClButton(false);
                ((Button)PART_ClButtonWithTitle).Click += OnBtActionBarClick;
                try
                {
                    ui.AddElementInCustomTheme(PART_ClButtonWithTitle, "PART_ClButtonWithTitle");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ClButtonWithTitle", Element = PART_ClButtonWithTitle });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", "Error on AddCustomElements()");
                }
            }

            if (PART_ClButtonWithTitleAndDetails != null)
            {
                PART_ClButtonWithTitleAndDetails = new ClButtonAdvanced(false);
                try
                {
                    ui.AddElementInCustomTheme(PART_ClButtonWithTitleAndDetails, "PART_ClButtonWithTitleAndDetails");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ClButtonWithTitleAndDetails", Element = PART_ClButtonWithTitleAndDetails });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", "Error on AddCustomElements()");
                }
            }

            if (PART_ClButtonWithJustIconAndDetails != null)
            {
                PART_ClButtonWithJustIconAndDetails = new ClButtonAdvanced(true);
                try
                {
                    ui.AddElementInCustomTheme(PART_ClButtonWithJustIconAndDetails, "PART_ClButtonWithJustIconAndDetails");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ClButtonWithJustIconAndDetails", Element = PART_ClButtonWithJustIconAndDetails });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", "Error on AddCustomElements()");
                }
            }

            if (PART_ClListLanguages != null)
            {
                PART_ClListLanguages = new ClDescriptionIntegration(false, true);
                try
                {
                    ui.AddElementInCustomTheme(PART_ClListLanguages, "PART_ClListLanguages");
                    ListCustomElements.Add(new CustomElement { ParentElementName = "PART_ClListLanguages", Element = PART_ClListLanguages });
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "CheckLocalizations", "Error on AddCustomElements()");
                }
            }
        }

        public override void RefreshCustomElements()
        {
            foreach (CustomElement customElement in ListCustomElements)
            {
                customElement.Element.Visibility = Visibility.Visible;

                if (customElement.Element is ClButtonAdvanced)
                {

                }

                if (customElement.Element is ClDescriptionIntegration)
                {

                }
            }
        }
        #endregion
    }
}
