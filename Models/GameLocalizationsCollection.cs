using Playnite.SDK;
using PluginCommon.PlayniteResources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Models
{
    public class GameLocalizationsCollection : ItemCollection<GameLocalizations>
    {
        public GameLocalizationsCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : base(path, type)
        {
        }
    }
}

