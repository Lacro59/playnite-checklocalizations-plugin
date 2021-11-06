using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPluginsShared.Collections;
using CommonPlayniteShared.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Models
{
    public class GameLocalizationsCollection : PluginItemCollection<GameLocalizations>
    {
        public GameLocalizationsCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : base(path, type)
        {
        }
    }
}
