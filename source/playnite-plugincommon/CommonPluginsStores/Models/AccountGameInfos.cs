using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CommonPluginsStores.Models
{
    public class AccountGameInfos : BasicAccountGameInfos
    {
        public bool IsCommun { get; set; }
        public long Playtime { get; set; }

        [DontSerialize]
        public int AchievementsUnlocked { get => Achievements?.Where(y => y.DateUnlocked != default)?.Count() ?? 0; }
        public ObservableCollection<GameAchievement> Achievements { get; set; }
    }
}
