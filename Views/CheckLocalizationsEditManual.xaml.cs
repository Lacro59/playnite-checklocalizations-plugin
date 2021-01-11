using CheckLocalizations.Models;
using CheckLocalizations.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static readonly ILogger logger = LogManager.GetLogger();

        private List<GameLanguage> gameLanguageAvailable = new List<GameLanguage>();
        private GameLocalizations _gameLocalizations;

        private Game _game;


        public CheckLocalizationsEditManual()
        {
            InitializeComponent();

            _gameLocalizations = CheckLocalizations.PluginDatabase.GameSelectedData;
            _game = LocalizationsDatabase.GameSelected;

#if DEBUG
            logger.Debug($"CheckLocalizations [Ignored] - EditManual All - {_game.Name} - _gameLocalizations: {JsonConvert.SerializeObject(_gameLocalizations)}");
#endif
            ListViewLanguages.ItemsSource = _gameLocalizations.Items.Where(x => x.IsManual).ToList();
#if DEBUG
            logger.Debug($"CheckLocalizations [Ignored] - EditManual IsManual only - {_game.Name} - _gameLocalizations: {JsonConvert.SerializeObject(ListViewLanguages.ItemsSource)}");
#endif
            RefreshAvailable();
        }


        private void RefreshAvailable()
        {
            gameLanguageAvailable = new List<GameLanguage>();
            foreach (GameLanguage gameLanguage in CheckLocalizations.GameLanguages)
            {
                if (_gameLocalizations.Items.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) == null)
                {
                    gameLanguageAvailable.Add(gameLanguage);
                }
            }

            gameLanguageAvailable.Sort((x, y) => x.Name.CompareTo(y.Name));

            PART_LocalizationSelection.ItemsSource = null;
            PART_LocalizationSelection.ItemsSource = gameLanguageAvailable;
            PART_LocalizationSelection.SelectedIndex = -1;
        }


        private void BtAdd_Click(object sender, RoutedEventArgs e)
        {
            string Language = ((GameLanguage)PART_LocalizationSelection.SelectedItem).Name;
            SupportStatus Ui = ((bool)PART_LocalizationUI.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative;
            SupportStatus Audio = ((bool)PART_LocalizationAudio.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative;
            SupportStatus Sub = ((bool)PART_LocalizationSub.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative;

            Localization localization = new Localization
            {
                Language = Language,
                Ui = Ui,
                Audio = Audio,
                Sub = Sub,
                Notes = string.Empty,
                IsManual = true,
            };

            int index = _gameLocalizations.Items.FindIndex(x => x.Language == Language);
            if (index > -1)
            {
                _gameLocalizations.Items[index] = localization;
#if DEBUG
                logger.Debug($"CheckLocalizations [Ignored] - BtAdd_Click({index}) - {_game.Name} - _gameLocalizations: {JsonConvert.SerializeObject(_gameLocalizations)}");
#endif
            }
            else
            {
                _gameLocalizations.Items.Add(localization);
#if DEBUG
                logger.Debug($"CheckLocalizations [Ignored] - BtAdd_Click({index}) - {_game.Name} - _gameLocalizations: {JsonConvert.SerializeObject(_gameLocalizations)}");
#endif
            }

            _gameLocalizations.Items.Sort((x, y) => x.Language.CompareTo(y.Language));

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = _gameLocalizations.Items.Where(x => x.IsManual).ToList();

            RefreshAvailable();
        }

        private void BtRemove_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            _gameLocalizations.Items.Remove((Localization)ListViewLanguages.Items[index]);

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = _gameLocalizations.Items.Where(x => x.IsManual).ToList();

            RefreshAvailable();
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            GameLocalizations gameLocalizations = CheckLocalizations.PluginDatabase.GetOnlyCache(_game.Id);

            if (gameLocalizations == null)
            {
                _gameLocalizations = CheckLocalizations.PluginDatabase.GetDefault(_game);

#if DEBUG
                logger.Debug($"CheckLocalizations [Ignored] - EditManual - Add: {JsonConvert.SerializeObject(_gameLocalizations)}");
#endif
                CheckLocalizations.PluginDatabase.Add(_gameLocalizations);
            }
            else
            {
#if DEBUG
                logger.Debug($"CheckLocalizations [Ignored] - EditManual - Update: {JsonConvert.SerializeObject(_gameLocalizations)}");
#endif
                CheckLocalizations.PluginDatabase.Update(_gameLocalizations);
            }

            var TaskIntegrationUI = Task.Run(() =>
            {
                CheckLocalizations.checkLocalizationsUI.RefreshElements(LocalizationsDatabase.GameSelected);
            });

            ((Window)this.Parent).Close();
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
