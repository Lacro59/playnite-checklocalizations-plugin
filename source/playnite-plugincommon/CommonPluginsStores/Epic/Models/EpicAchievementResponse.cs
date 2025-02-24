using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Epic.Models
{
    public class Achievement
    {
        [SerializationPropertyName("productAchievementsRecordBySandbox")]
        public ProductAchievementsRecordBySandbox ProductAchievementsRecordBySandbox { get; set; }
    }

    public class Achievement2
    {
        [SerializationPropertyName("achievement")]
        public Achievement3 Achievement { get; set; }
    }

    public class Achievement3
    {
        [SerializationPropertyName("sandboxId")]
        public string SandboxId { get; set; }

        [SerializationPropertyName("deploymentId")]
        public string DeploymentId { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("hidden")]
        public bool Hidden { get; set; }

        [SerializationPropertyName("isBase")]
        public bool IsBase { get; set; }

        [SerializationPropertyName("achievementSetId")]
        public string AchievementSetId { get; set; }

        [SerializationPropertyName("unlockedDisplayName")]
        public string UnlockedDisplayName { get; set; }

        [SerializationPropertyName("lockedDisplayName")]
        public string LockedDisplayName { get; set; }

        [SerializationPropertyName("unlockedDescription")]
        public string UnlockedDescription { get; set; }

        [SerializationPropertyName("lockedDescription")]
        public string LockedDescription { get; set; }

        [SerializationPropertyName("unlockedIconId")]
        public string UnlockedIconId { get; set; }

        [SerializationPropertyName("lockedIconId")]
        public string LockedIconId { get; set; }

        [SerializationPropertyName("XP")]
        public int XP { get; set; }

        [SerializationPropertyName("flavorText")]
        public string FlavorText { get; set; }

        [SerializationPropertyName("unlockedIconLink")]
        public string UnlockedIconLink { get; set; }

        [SerializationPropertyName("lockedIconLink")]
        public string LockedIconLink { get; set; }

        [SerializationPropertyName("tier")]
        public Tier Tier { get; set; }

        [SerializationPropertyName("rarity")]
        public Rarity Rarity { get; set; }
    }

    public class AchievementSet
    {
        [SerializationPropertyName("achievementSetId")]
        public string AchievementSetId { get; set; }

        [SerializationPropertyName("isBase")]
        public bool IsBase { get; set; }

        [SerializationPropertyName("totalAchievements")]
        public int? TotalAchievements { get; set; }

        [SerializationPropertyName("totalXP")]
        public int TotalXP { get; set; }
    }

    public class DataAchievement
    {
        [SerializationPropertyName("Achievement")]
        public Achievement Achievement { get; set; }
    }

    public class PlatinumRarity
    {
        [SerializationPropertyName("percent")]
        public float Percent { get; set; }
    }

    public class ProductAchievementsRecordBySandbox
    {
        [SerializationPropertyName("productId")]
        public string ProductId { get; set; }

        [SerializationPropertyName("sandboxId")]
        public string SandboxId { get; set; }

        [SerializationPropertyName("totalAchievements")]
        public int? TotalAchievements { get; set; }

        [SerializationPropertyName("totalProductXP")]
        public int? TotalProductXP { get; set; }

        [SerializationPropertyName("achievementSets")]
        public List<AchievementSet> AchievementSets { get; set; }

        [SerializationPropertyName("platinumRarity")]
        public PlatinumRarity PlatinumRarity { get; set; }

        [SerializationPropertyName("achievements")]
        public List<Achievement2> Achievements { get; set; }
    }

    public class Rarity
    {
        [SerializationPropertyName("percent")]
        public float Percent { get; set; }
    }

    public class EpicAchievementResponse
    {
        [SerializationPropertyName("data")]
        public DataAchievement Data { get; set; }

        //[SerializationPropertyName("extensions")]
        //public Extensions Extensions { get; set; }
    }

    public class Tier
    {
        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("hexColor")]
        public string HexColor { get; set; }

        [SerializationPropertyName("min")]
        public int Min { get; set; }

        [SerializationPropertyName("max")]
        public int Max { get; set; }
    }
}
