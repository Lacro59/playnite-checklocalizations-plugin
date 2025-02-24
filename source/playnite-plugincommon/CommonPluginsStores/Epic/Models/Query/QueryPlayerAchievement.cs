using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models.Query
{
    public class QueryPlayerAchievement
    {
        public class Variables
        {
            public string epicAccountId = "";
            public string sandboxId = "";
        }

        public Variables variables = new Variables();
        public string query = @"query PlayerAchievement($epicAccountId:String!,$sandboxId:String!){PlayerAchievement{playerAchievementGameRecordsBySandbox(epicAccountId:$epicAccountId sandboxId:$sandboxId){records{totalXP totalUnlocked playerAwards{awardType unlockedDateTime  achievementSetId} achievementSets{achievementSetId isBase totalUnlocked totalXP} playerAchievements{playerAchievement{sandboxId epicAccountId unlocked progress XP unlockDate achievementName isBase achievementSetId}}}}}}";
    }
}
