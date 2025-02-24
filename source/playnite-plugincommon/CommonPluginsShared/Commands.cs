using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace CommonPluginsShared
{
    public static class Commands
    {
        [Obsolete("Use CommonPlayniteShared.Commands.NavigateUrlCommand", true)]
        public static RelayCommand<object> NavigateUrl => new RelayCommand<object>((url) =>
        {
            try
            {
                if (url is string stringUrl)
                {
                    Process.Start(stringUrl);
                }
                else if (url is Uri uriUrl)
                {
                    Process.Start(uriUrl.AbsoluteUri);
                }
                else
                {
                    throw new Exception("Unsupported URL format.");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, "Failed to open url.");
            }
        });

        public static RelayCommand<object> RestartRequired => new RelayCommand<object>((sender) =>
        {
            try
            {
                Window WinParent = UI.FindParent<Window>((FrameworkElement)sender);
                if (WinParent.DataContext?.GetType().GetProperty("IsRestartRequired") != null)
                {
                    ((dynamic)WinParent.DataContext).IsRestartRequired = true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        });

        public static RelayCommand<Guid> GoToGame => new RelayCommand<Guid>((id) =>
        {
            API.Instance.MainView.SelectGame(id);
            API.Instance.MainView.SwitchToLibraryView();
        });
    }
}
