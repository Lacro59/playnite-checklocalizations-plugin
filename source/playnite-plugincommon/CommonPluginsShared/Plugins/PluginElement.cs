using Playnite.SDK;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CommonPluginsShared.PlayniteTools;

namespace CommonPluginsShared.Plugins
{
    public class PluginElement
    {
        private Guid PluginId { get; }
        private ExternalPlugin ExternalPlugin { get; }

        private Plugin _plugin;
        private Plugin plugin
        {
            get
            {
                if (_plugin == null)
                {
                    _plugin = API.Instance?.Addons?.Plugins?.FirstOrDefault(p => p.Id == PluginId) ?? null;
                }
                return _plugin;
            }
        }


        public PluginElement(ExternalPlugin ExternalPlugin)
        {
            this.ExternalPlugin = ExternalPlugin;
            this.PluginId = PlayniteTools.GetPluginId(ExternalPlugin);
        }


        public IEnumerable<GameMenuItem> GetGameMenuItems(List<Game> Games)
        {
            return plugin?.GetGameMenuItems(new GetGameMenuItemsArgs { Games = Games }) ?? null;
        }

        public IEnumerable<GameMenuItem> GetGameMenuItemsPurged(List<Game> Games)
        {
            IEnumerable<GameMenuItem> pluginsMenus = plugin?.GetGameMenuItems(new GetGameMenuItemsArgs { Games = Games }) ?? null;
            if (pluginsMenus != null)
            {
                switch (ExternalPlugin)
                {
                    case ExternalPlugin.SuccessStory:
                        for (int idx = pluginsMenus.Count() - 1; idx > -1; idx--)
                        {



                        }
                        break;
                }
            }
            return pluginsMenus;
        }
    }
}
