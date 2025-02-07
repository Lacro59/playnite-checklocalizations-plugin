using CheckLocalizations.Models;
using System;
using System.Windows.Controls;
using CheckLocalizations.Controls;
using CheckLocalizations.Services;
using System.Diagnostics;
using System.Windows.Documents;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : UserControl
    {
        private LocalizationsDatabase PluginDatabase => CheckLocalizations.PluginDatabase;


        public CheckLocalizationsView()
        {
            InitializeComponent();

            PluginListLanguages PART_ListViewLanguages = new PluginListLanguages();
            PART_ListViewLanguages.WithColNotes = true;
            PART_ListViewLanguages.IgnoreSettings = true;
            PART_ListViewLanguages.GameContext = PluginDatabase.GameContext;
            _ = PART_Contener.Children.Add(PART_ListViewLanguages);


            GameLocalizations gameLocalizations = PluginDatabase.Get(PluginDatabase.GameContext, true);

            if (gameLocalizations.SourcesLink != null)
            {
                PART_SourceLabel.Text = gameLocalizations.SourcesLink.GameName + " (" + gameLocalizations.SourcesLink.Name +")";
                PART_SourceLink.Tag = gameLocalizations.SourcesLink.Url;
            }
        }


        private void PART_SourceLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (((Hyperlink)sender).Tag is string link)
            {
                if (!link.IsNullOrEmpty())
                {
                    _ = Process.Start(link);
                }
            }
        }
    }
}
