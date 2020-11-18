using CheckLocalizations.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;

namespace CheckLocalizations.Views
{
    /// <summary>
    /// Logique d'interaction pour CheckLocalizationsView.xaml
    /// </summary>
    public partial class CheckLocalizationsView : UserControl
    {
        public CheckLocalizationsView(GameLocalizations gameLocalizations)
        {
            InitializeComponent();

            ListViewLanguages.ItemsSource = gameLocalizations.Data;
        }
    }
}
