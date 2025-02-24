using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
//using NLog;
//using NLog.Config;
//using NLog.Targets;
using System.Configuration;
using CommonPlayniteShared.Common;//using Playnite.Common;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
//using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
//using Playnite.Metadata;
using Playnite.SDK;
using Microsoft.Win32;
using Playnite.SDK.Models;
using System.Collections.ObjectModel;
using Playnite.SDK.Plugins;

namespace CommonPlayniteShared
{
    public enum ImageLoadScaling
    {
        //[Description(LOC.SettingsImageScalingQuality)]
        None,
        //[Description(LOC.SettingsImageScalingBalanced)]
        BitmapDotNet,
        //[Description(LOC.SettingsImageScalingAlternative)]
        Custom
    }

    public class PlayniteSettings : ObservableObject
    {
        public static bool IsPortable
        {
            get
            {
                return !File.Exists(PlaynitePaths.UninstallerPath);
            }
        }

        private string directoryOpenCommand;
        public string DirectoryOpenCommand
        {
            get => directoryOpenCommand;
            set
            {
                directoryOpenCommand = value;
                OnPropertyChanged();
            }
        }
    }
}
