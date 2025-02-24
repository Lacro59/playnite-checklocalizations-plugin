using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using static CommonPluginsShared.PlayniteTools;

namespace CommonPluginsShared.Plugins
{
    public class PluginMenu
    {
        private List<PluginElement> PluginElements { get; set; }


        public PluginMenu(List<ExternalPlugin> ExternalPlugins)
        {
            ExternalPlugins.ForEach(x =>
            {
                PluginElements = new List<PluginElement> { new PluginElement(x) };
            });
        }


        public IEnumerable<GameMenuItem> GetGameMenuItems(List<Game> Games, bool PurgeMenus = true)
        {
            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();
            PluginElements.ForEach(x => 
            {
                IEnumerable<GameMenuItem> PluginMenus = PurgeMenus ? x.GetGameMenuItemsPurged(Games) : x.GetGameMenuItems(Games);
                if (PluginMenus != null)
                {
                    gameMenuItems = gameMenuItems.Concat(PluginMenus).ToList();
                }
            });
            return gameMenuItems;
        }
    }
}
