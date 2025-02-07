using CheckLocalizations.Models;
using CheckLocalizations.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Localization = CheckLocalizations.Models.Localization;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsEditManual.xaml
    /// </summary>
    public partial class CheckLocalizationsEditManual : UserControl
    {
        private static ILogger Logger => LogManager.GetLogger();

        private static LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;

        private List<GameLanguage> GameLanguageAvailable { get; set; } = new List<GameLanguage>();
        private GameLocalizations GameLocalizations { get; set; }

        private Game GameContext { get; set; }


        public CheckLocalizationsEditManual(Game game)
        {
            InitializeComponent();

            GameLocalizations = PluginDatabase.Get(game);
            GameContext = PluginDatabase.GameContext;

            ListViewLanguages.ItemsSource = GameLocalizations.Items.Where(x => x.IsManual).ToList();

            RefreshAvailable();
        }


        private void RefreshAvailable()
        {
            GameLanguageAvailable = new List<GameLanguage>();
            foreach (GameLanguage gameLanguage in CheckLocalizations.PluginDatabase.PluginSettings.Settings.GameLanguages)
            {
                if (GameLocalizations.Items.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) == null)
                {
                    GameLanguageAvailable.Add(gameLanguage);
                }
            }

            GameLanguageAvailable.Sort((x, y) => x.Name.CompareTo(y.Name));

            PART_LocalizationSelection.ItemsSource = null;
            PART_LocalizationSelection.ItemsSource = GameLanguageAvailable;
            PART_LocalizationSelection.SelectedIndex = -1;
        }


        private void BtAdd_Click(object sender, RoutedEventArgs e)
        {
            string language = ((GameLanguage)PART_LocalizationSelection.SelectedItem).Name;
            SupportStatus ui = ((bool)PART_LocalizationUI.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative;
            SupportStatus audio = ((bool)PART_LocalizationAudio.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative;
            SupportStatus sub = ((bool)PART_LocalizationSub.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative;

            Localization localization = new Localization
            {
                Language = language,
                Ui = ui,
                Audio = audio,
                Sub = sub,
                Notes = string.Empty,
                IsManual = true,
            };

            int index = GameLocalizations.Items.FindIndex(x => x.Language == language);
            if (index > -1)
            {
                GameLocalizations.Items[index] = localization;
            }
            else
            {
                GameLocalizations.Items.Add(localization);
            }

            GameLocalizations.Items.Sort((x, y) => x.Language.CompareTo(y.Language));

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = GameLocalizations.Items.Where(x => x.IsManual).ToList();

            RefreshAvailable();
        }

        private void BtRemove_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            _ = GameLocalizations.Items.Remove((Localization)ListViewLanguages.Items[index]);

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = GameLocalizations.Items.Where(x => x.IsManual).ToList();

            RefreshAvailable();
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            GameLocalizations gameLocalizations = PluginDatabase.GetOnlyCache(GameContext.Id);

            if (gameLocalizations == null)
            {
                GameLocalizations = CheckLocalizations.PluginDatabase.GetDefault(GameContext);
                PluginDatabase.Add(GameLocalizations);
            }
            else
            {
                PluginDatabase.Update(GameLocalizations);
            }

            ((Window)Parent).Close();
        }


        private void PART_LocalizationSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PART_LocalizationAdd.IsEnabled = true;
            if (PART_LocalizationSelection.SelectedIndex == -1)
            {
                PART_LocalizationAdd.IsEnabled = false;
            }
        }
    }
}
