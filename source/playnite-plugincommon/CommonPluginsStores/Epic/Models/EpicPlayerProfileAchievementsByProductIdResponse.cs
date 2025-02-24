using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models
{
    public class Avatar
    {
        [SerializationPropertyName("small")]
        public string Small { get; set; }

        [SerializationPropertyName("medium")]
        public string Medium { get; set; }

        [SerializationPropertyName("large")]
        public string Large { get; set; }
    }

    public class DataAchievementsByProductId
    {
        [SerializationPropertyName("PlayerProfile")]
        public PlayerProfile PlayerProfile { get; set; }
    }

    public class DataProductAchievements
    {
        [SerializationPropertyName("PlayerProfile")]
        public PlayerProfile PlayerProfile { get; set; }

        [SerializationPropertyName("epicAccountId")]
        public string EpicAccountId { get; set; }

        [SerializationPropertyName("sandboxId")]
        public string SandboxId { get; set; }

        [SerializationPropertyName("totalXP")]
        public int TotalXP { get; set; }

        [SerializationPropertyName("totalUnlocked")]
        public int TotalUnlocked { get; set; }

        [SerializationPropertyName("achievementSets")]
        public List<AchievementSet> AchievementSets { get; set; }

        [SerializationPropertyName("playerAwards")]
        public List<object> PlayerAwards { get; set; }

        [SerializationPropertyName("playerAchievements")]
        public List<PlayerAchievements> PlayerAchievements { get; set; }
    }

    public class PlayerAchievements
    {
        [SerializationPropertyName("playerAchievement")]
        public PlayerAchievement3 PlayerAchievement { get; set; }
    }

    public class PlayerProfile
    {
        [SerializationPropertyName("playerProfile")]
        public PlayerProfile2 PlayerProfile2 { get; set; }
    }

    public class PlayerProfile2
    {
        [SerializationPropertyName("epicAccountId")]
        public string EpicAccountId { get; set; }

        [SerializationPropertyName("displayName")]
        public string DisplayName { get; set; }

        [SerializationPropertyName("relationship")]
        public string Relationship { get; set; }

        [SerializationPropertyName("avatar")]
        public Avatar Avatar { get; set; }

        [SerializationPropertyName("productAchievements")]
        public ProductAchievements ProductAchievements { get; set; }
    }

    public class ProductAchievements
    {
        [SerializationPropertyName("data")]
        public DataProductAchievements Data { get; set; }
    }

    public class EpicPlayerProfileAchievementsByProductIdResponse
    {
        [SerializationPropertyName("data")]
        public DataAchievementsByProductId Data { get; set; }
    }
}
