using CheckLocalizations.Models;
using Playnite.SDK;
using PluginCommon;
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

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClButtonAdvanced.xaml
    /// </summary>
    public partial class ClButtonAdvanced : Button
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        string IsOk = "";
        string IsKo = "";


        public ClButtonAdvanced(bool SupportNative, List<GameLocalization> gameLocalizations)
        {
            InitializeComponent();

            if (SupportNative)
            {
                IndicatorSupport.Text = IsOk;
            }
            else
            {
                IndicatorSupport.Text = IsKo;
            }

            ListViewLanguages.ItemsSource = gameLocalizations;
        }

        private void ListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)((ListView)sender).Parent))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
        }
    }
}
