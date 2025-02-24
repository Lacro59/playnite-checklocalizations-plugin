using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamAchievements
    {
        [SerializationPropertyName("response")]
        public AchievementsResponse Response { get; set; }
    }

    public class Achievement
    {
        [SerializationPropertyName("internal_name")]
        public string InternalName { get; set; }
        [SerializationPropertyName("localized_name")]
        public string LocalizedName { get; set; }
        [SerializationPropertyName("localized_desc")]
        public string LocalizedDesc { get; set; }
        [SerializationPropertyName("icon")]
        public string Icon { get; set; }
        [SerializationPropertyName("icon_gray")]
        public string IconGray { get; set; }
        [SerializationPropertyName("hidden")]
        public bool Hidden { get; set; }
        [SerializationPropertyName("player_percent_unlocked")]
        public string PlayerPercentUnlocked { get; set; }
    }

    public class AchievementsResponse
    {
        [SerializationPropertyName("achievements")]
        public List<Achievement> Achievements { get; set; }
    }
}
