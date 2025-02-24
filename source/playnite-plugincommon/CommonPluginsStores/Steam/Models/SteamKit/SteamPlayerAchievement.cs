using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models.SteamKit
{
    public class SteamPlayerAchievement
    {
        public string ApiName { get; set; }
        public int Achieved { get; set; }
        public DateTime UnlockTime { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
