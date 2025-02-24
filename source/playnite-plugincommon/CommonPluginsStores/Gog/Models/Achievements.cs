using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Gog.Models
{
    public class Achievements
    {
        [SerializationPropertyName("total_count")]
        public int TotalCount { get; set; }

        [SerializationPropertyName("limit")]
        public int Limit { get; set; }

        [SerializationPropertyName("page_token")]
        public string PageToken { get; set; }

        [SerializationPropertyName("items")]
        public List<AchItem> Items { get; set; }

        [SerializationPropertyName("achievements_mode")]
        public string AchievementsMode { get; set; }
    }

    public class AchItem
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("achievement_id")]
        public string AchievementId { get; set; }

        [SerializationPropertyName("achievement_key")]
        public string AchievementKey { get; set; }

        [SerializationPropertyName("visible")]
        public bool Visible { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("description")]
        public string Description { get; set; }

        [SerializationPropertyName("image_url_unlocked")]
        public string ImageUrlUnlocked { get; set; }

        [SerializationPropertyName("imageUrlUnlocked")]
        public string ImageUrlUnlocked2 { get; set; }

        [SerializationPropertyName("image_url_locked")]
        public string ImageUrlLocked { get; set; }

        [SerializationPropertyName("imageUrlLocked")]
        public string ImageUrlLocked2 { get; set; }

        [SerializationPropertyName("rarity")]
        public double Rarity { get; set; }

        [SerializationPropertyName("date_unlocked")]
        public DateTime? DateUnlocked { get; set; }

        [SerializationPropertyName("rarity_level_description")]
        public string RarityLevelDescription { get; set; }

        [SerializationPropertyName("rarity_level_slug")]
        public string RarityLevelSlug { get; set; }
    }
}
