using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models.Query
{
    public class QueryAchievement
    {
        public class Variables
        {
            public string locale = "en-US";
            public string sandboxId = "";
        }

        public Variables variables = new Variables();
        public string query = @"query Achievement($sandboxId:String!,$locale:String!){Achievement{productAchievementsRecordBySandbox(sandboxId:$sandboxId,locale:$locale){productId sandboxId totalAchievements totalProductXP achievementSets{achievementSetId isBase totalAchievements totalXP} platinumRarity{percent} achievements{achievement{sandboxId deploymentId name hidden isBase achievementSetId unlockedDisplayName lockedDisplayName unlockedDescription lockedDescription unlockedIconId lockedIconId XP flavorText unlockedIconLink lockedIconLink tier{name hexColor min max} rarity{percent}}}}}}";
    }
}
