﻿using CommonPluginsShared.Collections;
using System.Collections.Generic;
using CommonPluginsShared.Models;

namespace CheckLocalizations.Models
{
    public class GameLocalizations : PluginDataBaseGame<Localization>
    {
        private List<Localization> _Items = new List<Localization>();
        public override List<Localization> Items { get => _Items; set => SetValue(ref _Items, value); }


        public bool HasChecked { get; set; }

        public SourceLink SourcesLink { get; set; }

        /// <summary>
        /// Indicates whether one of the languages ​​is the native language
        /// </summary>
        /// <returns></returns>
        public bool HasNativeSupport()
        {
            foreach (GameLanguage gameLanguage in CheckLocalizations.PluginDatabase.PluginSettings.Settings.GameLanguages)
            {
                if (gameLanguage.IsNative)
                {
                    if (Items.Find(x => x.Language.ToLower() == gameLanguage.Name.ToLower()) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
