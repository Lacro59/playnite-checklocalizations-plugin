using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamFriendsApps
    {
        [SerializationPropertyName("appid")]
        public int AppId { get; set; }
        [SerializationPropertyName("name")]
        public string Name { get; set; }
        [SerializationPropertyName("app_type")]
        public int AppType { get; set; }
        [SerializationPropertyName("logo")]
        public string Logo { get; set; }
        [SerializationPropertyName("friendlyURL")]
        public object FriendlyURL { get; set; }
        [SerializationPropertyName("availStatLinks")]
        public AvailStatLinks AvailStatLinks { get; set; }
        [SerializationPropertyName("hours_forever")]
        public string HoursForever { get; set; }
        [SerializationPropertyName("last_played")]
        public int LastPlayed { get; set; }
        [SerializationPropertyName("has_adult_content")]
        public int? HasAdultContent { get; set; }
        [SerializationPropertyName("friendly_name")]
        public string FriendlyName { get; set; }
        [SerializationPropertyName("hours")]
        public string Hours { get; set; }
        [SerializationPropertyName("is_visible_in_steam_china")]
        public int? IsVisibleInSteamChina { get; set; }
    }

    public class AvailStatLinks
    {
        [SerializationPropertyName("achievements")]
        public bool Achievements { get; set; }
        [SerializationPropertyName("global_achievements")]
        public bool GlobalAchievements { get; set; }
        [SerializationPropertyName("stats")]
        public bool Stats { get; set; }
        [SerializationPropertyName("gcpd")]
        public bool Gcpd { get; set; }
        [SerializationPropertyName("leaderboards")]
        public bool Leaderboards { get; set; }
        [SerializationPropertyName("global_leaderboards")]
        public bool GlobalLeaderboards { get; set; }
    }

}
