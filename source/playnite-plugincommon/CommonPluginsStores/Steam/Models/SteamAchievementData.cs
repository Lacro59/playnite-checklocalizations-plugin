using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamAchievementData
    {
        [SerializationPropertyName("rawname")]
        public string RawName { get; set; }
        [SerializationPropertyName("hidden")]
        public bool Hidden { get; set; }
        [SerializationPropertyName("closed")]
        public int Closed { get; set; }
        [SerializationPropertyName("unlock_time")]
        public int UnlockTime { get; set; }
        [SerializationPropertyName("icon_closed")]
        public string IconClosed { get; set; }
        [SerializationPropertyName("icon_open")]
        public string IconOpen { get; set; }
        [SerializationPropertyName("progress")]
        public dynamic Progress { get; set; }
        [SerializationPropertyName("name")]
        public string Name { get; set; }
        [SerializationPropertyName("desc")]
        public string Desc { get; set; }
    }
}
