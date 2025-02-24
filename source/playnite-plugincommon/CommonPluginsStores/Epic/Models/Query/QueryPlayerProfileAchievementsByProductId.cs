using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models.Query
{
    public class QueryPlayerProfileAchievementsByProductId
    {
        public class Variables
        {
            public string epicAccountId = "";
            public string productId = "";
        }

        public Variables variables = new Variables();
        public string query = @"query playerProfileAchievementsByProductId($epicAccountId:String!,$productId:String!){PlayerProfile{playerProfile(epicAccountId:$epicAccountId){epicAccountId displayName relationship avatar{small medium large} productAchievements(productId:$productId){...on PlayerProductAchievementsResponseSuccess{data{epicAccountId sandboxId totalXP totalUnlocked achievementSets{achievementSetId isBase totalUnlocked totalXP} playerAwards{awardType unlockedDateTime achievementSetId} playerAchievements{playerAchievement{achievementName epicAccountId progress sandboxId unlocked unlockDate XP achievementSetId isBase}}}}}}}}";
    }
}
