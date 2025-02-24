using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Playnite.SDK;

namespace CommonPluginsShared
{
    public class PlayniteUiHelper
    {
        /// <summary>
        /// Can to exit window with escape key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (sender is Window window)
                {
                    e.Handled = true;
                    window.Close();
                }
            }
        }

        /// <summary>
        /// Create Window with Playnite SDK
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="ViewExtension"></param>
        /// <param name="windowOptions"></param>
        /// <returns></returns>
        public static Window CreateExtensionWindow(string Title, UserControl ViewExtension, WindowOptions windowOptions = null)
        {
            // Default window options
            if (windowOptions == null)
            {
                windowOptions = new WindowOptions
                {
                    ShowMinimizeButton = false,
                    ShowMaximizeButton = false,
                    ShowCloseButton = true
                };
            }

            Window windowExtension = API.Instance.Dialogs.CreateWindow(windowOptions);

            windowExtension.Title = Title;
            windowExtension.ShowInTaskbar = false;
            windowExtension.ResizeMode = windowOptions.CanBeResizable ? ResizeMode.CanResize : ResizeMode.NoResize;
            windowExtension.Owner = API.Instance.Dialogs.GetCurrentAppWindow();
            windowExtension.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowExtension.Content = ViewExtension;

            // TODO Still useful to add margin?
            if (!double.IsNaN(ViewExtension.Height) && !double.IsNaN(ViewExtension.Width))
            {
                windowExtension.Height = ViewExtension.Height + 25;
                windowExtension.Width = ViewExtension.Width;
            }
            else if (!double.IsNaN(ViewExtension.MinHeight) && !double.IsNaN(ViewExtension.MinWidth) && ViewExtension.MinHeight > 0 && ViewExtension.MinWidth > 0)
            {
                windowExtension.Height = ViewExtension.MinHeight + 25;
                windowExtension.Width = ViewExtension.MinWidth;
            }
            else if (windowOptions.Width != 0 && windowOptions.Height != 0)
            {
                windowExtension.Width = windowOptions.Width;
                windowExtension.Height = windowOptions.Height;
            }
            else
            {
                // TODO A black border is visible; SDK problem?
                windowExtension.SizeToContent = SizeToContent.WidthAndHeight;
            }

            //if (windowExtension.ResizeMode == ResizeMode.NoResize)
            //{
            //    windowExtension.SizeToContent = SizeToContent.WidthAndHeight;
            //}

            // Add escape event
            windowExtension.PreviewKeyDown += new KeyEventHandler(HandleEsc);

            return windowExtension;
        }
    }


    public class WindowOptions : WindowCreationOptions
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public bool CanBeResizable { get; set; } = false;
    }
}
