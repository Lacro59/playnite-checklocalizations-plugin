using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CommonPlayniteShared.PluginLibrary.PSNLibrary.Models
{
    public class PSNLibrarySettings : ObservableObject
    {
        public bool connectAccount = true;
        public bool downloadImageMetadata = true;
        public bool lastPlayed = true;
        public bool playtime = true;
        public bool playCount = true;
        public bool ps3 = true;
        public bool psp = true;
        public bool psvita = true;
        public bool pc = true;
        public bool migration = true;
        public bool tags = true;
        public bool noTags = false;
        public bool plusSource = false;
        private string npsso = string.Empty;

        public bool ConnectAccount { get => connectAccount; set => SetValue(ref connectAccount, value); }
        public bool DownloadImageMetadata { get => downloadImageMetadata; set => SetValue(ref downloadImageMetadata, value); }
        public bool LastPlayed { get => lastPlayed; set => SetValue(ref lastPlayed, value); }
        public bool Playtime { get => playtime; set => SetValue(ref playtime, value); }
        public bool PlayCount { get => playCount; set => SetValue(ref playCount, value); }
        public bool PS3 { get => ps3; set => SetValue(ref ps3, value); }
        public bool PSP { get => psp; set => SetValue(ref psp, value); }
        public bool PSVITA { get => psvita; set => SetValue(ref psvita, value); }
        public bool PC { get => pc; set => SetValue(ref pc, value); }
        public bool Migration { get => migration; set => SetValue(ref migration, value); }
        public bool Tags { get => tags; set => SetValue(ref tags, value); }
        public bool NoTags { get => noTags; set => SetValue(ref noTags, value); }
        public bool PlusSource { get => plusSource; set => SetValue(ref plusSource, value); }
        public string Npsso { get => npsso; set => SetValue(ref npsso, value); }
    }
}
