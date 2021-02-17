using CheckLocalizations.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using CheckLocalizations.Controls;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : UserControl
    {
        public CheckLocalizationsView()
        {
            InitializeComponent();

            CheckLocListLanguages PART_ListViewLanguages = new CheckLocListLanguages();
            PART_ListViewLanguages.WithColNotes = true;
            PART_ListViewLanguages.IgnoreSettings = true;
            PART_ListViewLanguages.GameContext = CheckLocalizations.PluginDatabase.GameContext;
            PART_Contener.Children.Add(PART_ListViewLanguages);
        }
    }
}
