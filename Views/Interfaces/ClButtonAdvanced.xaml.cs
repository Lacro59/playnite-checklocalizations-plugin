using CheckLocalizations.Models;
using Playnite.SDK;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.API;
using PluginCommon.PlayniteResources.Common;
using PluginCommon.PlayniteResources.Converters;
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

        private static readonly string IsOk = "";
        private static readonly string IsKo = "";
        private static readonly string IsNone = "";

        private static readonly string OnlyIconIsOk = "";
        private static readonly string OnlyIconIsKo = "";
        private static readonly string OnlyIconIsNone = "";


        public ClButtonAdvanced(bool SupportNative, List<GameLocalization> gameLocalizations, bool EnableIntegrationButtonJustIcon)
        {
            InitializeComponent();

            if (EnableIntegrationButtonJustIcon)
            {
                OnlyIcon.Visibility = Visibility.Visible;
                IndicatorSupport.Visibility = Visibility.Collapsed;
                IndicatorSupportText.Visibility = Visibility.Collapsed;
            }
            else
            {
                OnlyIcon.Visibility = Visibility.Collapsed;
                IndicatorSupport.Visibility = Visibility.Visible;
                IndicatorSupportText.Visibility = Visibility.Visible;
            }

            if (SupportNative)
            {
                IndicatorSupport.Text = IsOk;
                OnlyIcon.Text = OnlyIconIsOk;
            }
            else
            {
                IndicatorSupport.Text = IsKo;
                OnlyIcon.Text = OnlyIconIsKo;
            }

            if (gameLocalizations.Count == 0)
            {
                IndicatorSupport.Text = IsNone;
                OnlyIcon.Text = OnlyIconIsNone;
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
