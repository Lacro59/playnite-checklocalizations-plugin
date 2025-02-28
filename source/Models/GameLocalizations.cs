using CommonPluginsShared.Collections;
using System.Collections.Generic;
using CommonPluginsShared.Models;
using System.Linq;
using System;

namespace CheckLocalizations.Models
{
    public class GameLocalizations : PluginDataBaseGame<Localization>
    {
        private List<Localization> _items = new List<Localization>();
        public override List<Localization> Items { get => _items; set => SetValue(ref _items, value); }


        public bool HasChecked { get; set; }

        public SourceLink SourcesLink { get; set; }

        /// <summary>
        /// Indicates whether one of the languages ​​is the native language
        /// </summary>
        /// <returns></returns>
        public bool HasNativeSupport()
        {
            return CheckLocalizations.PluginDatabase.PluginSettings.Settings.GameLanguages
                .Where(gameLanguage => gameLanguage.IsNative)
                .Any(gameLanguage => Items
                    .Any(item => string.Equals(item.Language, gameLanguage.Name, StringComparison.OrdinalIgnoreCase)));
        }

        public bool HasManual()
        {
            return Items?.Count(x => x.IsManual) > 0;
        }
    }
}
