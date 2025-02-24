using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Steam.Models
{
    public class Datum
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("achievementApiNames")]
        public List<string> AchievementApiNames { get; set; }

        [SerializationPropertyName("dlcAppId")]
        public int? DlcAppId { get; set; }

        [SerializationPropertyName("dlcAppName")]
        public string DlcAppName { get; set; }
    }

    public class ExtensionsAchievements
    {
        [SerializationPropertyName("success")]
        public bool Success { get; set; }

        [SerializationPropertyName("data")]
        public List<Datum> Data { get; set; }
    }
}
