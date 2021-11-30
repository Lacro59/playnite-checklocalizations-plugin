using Playnite.SDK;
using CommonPluginsShared.Collections;

namespace CheckLocalizations.Models
{
    public class GameLocalizationsCollection : PluginItemCollection<GameLocalizations>
    {
        public GameLocalizationsCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : base(path, type)
        {
        }
    }
}
