using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models.SteamKit
{
    public class SteamSchema
    {
        public List<SteamSchemaAchievements> Achievements { get; set; } = new List<SteamSchemaAchievements>();
        public List<SteamSchemaStats> Stats { get; set; } = new List<SteamSchemaStats>();
    }

    public class SteamSchemaStats
    {
        public string Name { get; set; }
        public int DefaultValue { get; set; }
        public string DisplayName { get; set; }
    }

    public class SteamSchemaAchievements
    {
        public string Name { get; set; }
        public int DefaultValue { get; set; }
        public string DisplayName { get; set; }
        public bool Hidden { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string IconGray { get; set; }
    }
}
