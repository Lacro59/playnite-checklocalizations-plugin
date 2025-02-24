using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Epic.Models
{
    public class DataPlayerAchievement
    {
        [SerializationPropertyName("PlayerAchievement")]
        public PlayerAchievement PlayerAchievement { get; set; }
    }

    public class PlayerAchievement
    {
        [SerializationPropertyName("playerAchievementGameRecordsBySandbox")]
        public PlayerAchievementGameRecordsBySandbox PlayerAchievementGameRecordsBySandbox { get; set; }
    }

    public class PlayerAchievement2
    {
        [SerializationPropertyName("playerAchievement")]
        public PlayerAchievement3 PlayerAchievement { get; set; }
    }

    public class PlayerAchievement3
    {
        [SerializationPropertyName("sandboxId")]
        public string SandboxId { get; set; }

        [SerializationPropertyName("epicAccountId")]
        public string EpicAccountId { get; set; }

        [SerializationPropertyName("unlocked")]
        public bool Unlocked { get; set; }

        [SerializationPropertyName("progress")]
        public float Progress { get; set; }

        [SerializationPropertyName("XP")]
        public int XP { get; set; }

        [SerializationPropertyName("unlockDate")]
        public DateTime UnlockDate { get; set; }

        [SerializationPropertyName("achievementName")]
        public string AchievementName { get; set; }

        [SerializationPropertyName("isBase")]
        public bool IsBase { get; set; }

        [SerializationPropertyName("achievementSetId")]
        public string AchievementSetId { get; set; }
    }

    public class PlayerAchievementGameRecordsBySandbox
    {
        [SerializationPropertyName("records")]
        public List<Record> Records { get; set; }
    }

    public class Record
    {
        [SerializationPropertyName("totalXP")]
        public int TotalXP { get; set; }

        [SerializationPropertyName("totalUnlocked")]
        public int TotalUnlocked { get; set; }

        [SerializationPropertyName("playerAwards")]
        public List<object> PlayerAwards { get; set; }

        [SerializationPropertyName("achievementSets")]
        public List<AchievementSet> AchievementSets { get; set; }

        [SerializationPropertyName("playerAchievements")]
        public List<PlayerAchievement2> PlayerAchievements { get; set; }
    }

    public class EpicPlayerAchievementResponse
    {
        [SerializationPropertyName("data")]
        public DataPlayerAchievement Data { get; set; }
    }


}
