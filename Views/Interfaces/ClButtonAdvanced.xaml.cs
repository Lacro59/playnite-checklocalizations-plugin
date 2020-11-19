﻿using CheckLocalizations.Models;
using Playnite.SDK;
using PluginCommon;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
        }

        public void SetGameLocalizations(List<Models.Localization> gameLocalizations, bool SupportNative)
        {
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

            PART_ListViewLanguages.SetGameLocalizations(gameLocalizations);
        }

        // Design popup
        private void PART_ContextMenu_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)(sender)))
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
