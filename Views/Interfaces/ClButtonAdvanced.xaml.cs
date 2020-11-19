using CheckLocalizations.Models;
using Playnite.SDK;
using PluginCommon;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClButtonAdvanced.xaml
    /// </summary>
    public partial class ClButtonAdvanced : Button
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private static readonly string IsTextDefault = " ";
        private static readonly string IsTextOk = "";
        private static readonly string IsTextKo = "";
        private static readonly string IsTextNone = "";

        private static readonly string OnlyIconIsDefault = "";
        private static readonly string OnlyIconIsOk = "";
        private static readonly string OnlyIconIsKo = "";
        private static readonly string OnlyIconIsNone = "";

        private ClListViewLanguages PART_ListViewLanguages;


        public ClButtonAdvanced(bool EnableIntegrationButtonJustIcon)
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

            PART_ListViewLanguages = new ClListViewLanguages(true);
            PART_ContextMenu.Items.Add(PART_ListViewLanguages);

            CheckLocalizations.PluginDatabase.PropertyChanged += OnPropertyChanged;
        }

        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    if (CheckLocalizations.PluginDatabase.GameIsLoaded)
                    {
                        if ((bool)resources.GetResource("Cl_HasNativeSupport"))
                        {
                            IndicatorSupport.Text = IsTextOk;
                            OnlyIcon.Text = OnlyIconIsOk;
                        }
                        else
                        {
                            IndicatorSupport.Text = IsTextKo;
                            OnlyIcon.Text = OnlyIconIsKo;
                        }
                        
                        if (CheckLocalizations.PluginDatabase.GameSelectedData.Data.Count == 0)
                        {
                            IndicatorSupport.Text = IsTextNone;
                            OnlyIcon.Text = OnlyIconIsNone;
                        }
                    }
                    else
                    {
                        IndicatorSupport.Text = IsTextDefault;
                        OnlyIcon.Text = OnlyIconIsDefault;
                    }
                }));
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "CheckLocalizations");
            }
        }

        // Design popup
        private void PART_ContextMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            /*
            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)(sender)))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
            */
        }
    }
}
