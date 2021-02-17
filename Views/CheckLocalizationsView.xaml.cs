using CheckLocalizations.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using CheckLocalizations.Controls;
using CheckLocalizations.Services;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : UserControl
    {
        private LocalizationsDatabase PluginDatabase = CheckLocalizations.PluginDatabase;


        public CheckLocalizationsView()
        {
            InitializeComponent();

            CheckLocListLanguages PART_ListViewLanguages = new CheckLocListLanguages();
            PART_ListViewLanguages.WithColNotes = true;
            PART_ListViewLanguages.IgnoreSettings = true;
            PART_ListViewLanguages.GameContext = PluginDatabase.GameContext;
            PART_Contener.Children.Add(PART_ListViewLanguages);
        }
    }
}
