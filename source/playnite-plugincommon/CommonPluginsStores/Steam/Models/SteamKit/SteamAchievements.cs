using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models.SteamKit
{
    public class SteamAchievements
    {
        public string InternalName { get; set; }
        public string LocalizedName { get; set; }
        public string LocalizedDesc { get; set; }
        public string Icon { get; set; }
        public string IconGray { get; set; }
        public bool Hidden { get; set; }
        public float PlayerPercentUnlocked { get; set; }
    }
}
