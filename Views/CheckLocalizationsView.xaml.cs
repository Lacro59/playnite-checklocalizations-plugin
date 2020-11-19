using CheckLocalizations.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using CheckLocalizations.Views.Interfaces;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : UserControl
    {
        private ClListViewLanguages PART_ListViewLanguages;

        public CheckLocalizationsView(GameLocalizations gameLocalizations)
        {
            InitializeComponent();

            PART_ListViewLanguages = new ClListViewLanguages(true);
            PART_ListViewLanguages.SetGameLocalizations(gameLocalizations.Data);

            PART_LvContener.Children.Add(PART_ListViewLanguages);
        }
    }
}
