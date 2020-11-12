using CheckLocalizations.Models;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsEditManual.xaml
    /// </summary>
    public partial class CheckLocalizationsEditManual : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private List<GameLanguage> gameLanguageAvailable = new List<GameLanguage>();
        private List<GameLocalization> _gameLocalizations = new List<GameLocalization>();
        private List<GameLocalization> _gameLocalizationsManual = new List<GameLocalization>();

        private Game _game;


        public CheckLocalizationsEditManual(List<GameLocalization> gameLocalizations, List<GameLocalization> gameLocalizationsManual, Game game)
        {
#if DEBUG
            logger.Debug($"CheckLocalizations - gameLocalizationsManual: {JsonConvert.SerializeObject(gameLocalizationsManual)}");
#endif
            InitializeComponent();

            _gameLocalizations = gameLocalizations;

            _gameLocalizationsManual = gameLocalizationsManual;
            _gameLocalizationsManual.Sort((x, y) => x.Language.CompareTo(y.Language));

            _game = game;

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = _gameLocalizationsManual;

            RefreshAvailable();
        }

        private void RefreshAvailable()
        {
#if DEBUG
            logger.Debug($"CheckLocalizations - GameLanguages: {JsonConvert.SerializeObject(CheckLocalizations.GameLanguages)}");
            logger.Debug($"CheckLocalizations - gameLocalizations: {JsonConvert.SerializeObject(_gameLocalizations)}");
            logger.Debug($"CheckLocalizations - gameLocalizationsManual: {JsonConvert.SerializeObject(_gameLocalizationsManual)}");
#endif
            gameLanguageAvailable = new List<GameLanguage>();
            foreach (GameLanguage gameLanguage in CheckLocalizations.GameLanguages)
            {
                if (_gameLocalizationsManual.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) == null && _gameLocalizations.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) == null)
                {
                    gameLanguageAvailable.Add(gameLanguage);
                }
            }

            gameLanguageAvailable.Sort((x, y) => x.Name.CompareTo(y.Name));

#if DEBUG
            logger.Debug($"CheckLocalizations - gameLanguageAvailable: {JsonConvert.SerializeObject(gameLanguageAvailable)}");
#endif

            PART_LocalizationSelection.ItemsSource = null;
            PART_LocalizationSelection.ItemsSource = gameLanguageAvailable;
            PART_LocalizationSelection.SelectedIndex = -1;
        }

        private void BtAdd_Click(object sender, RoutedEventArgs e)
        {
            _gameLocalizationsManual.Add(new GameLocalization
            {
                Language = ((GameLanguage)PART_LocalizationSelection.SelectedItem).Name,
                Ui = ((bool)PART_LocalizationUI.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative,
                Audio = ((bool)PART_LocalizationAudio.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative,
                Sub = ((bool)PART_LocalizationSub.IsChecked) ? SupportStatus.Native : SupportStatus.NoNative,
            });
            
            _gameLocalizationsManual.Sort((x, y) => x.Language.CompareTo(y.Language));

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = _gameLocalizationsManual;

            RefreshAvailable();
        }

        private void BtRemove_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse(((Button)sender).Tag.ToString());

            _gameLocalizationsManual.RemoveAt(index);

            ListViewLanguages.ItemsSource = null;
            ListViewLanguages.ItemsSource = _gameLocalizationsManual;

            RefreshAvailable();
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            CheckLocalizations.localizationsApi.SaveLocalizationsManual(_gameLocalizationsManual, _game);

            var TaskIntegrationUI = Task.Run(() =>
            {
                CheckLocalizations.checkLocalizationsUI.RefreshElements(CheckLocalizations.GameSelected);
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
