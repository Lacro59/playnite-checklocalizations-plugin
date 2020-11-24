using Newtonsoft.Json;
using PluginCommon.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Models
{
    public class GameLocalizations : PluginDataBaseGame<Localization>
    {
        private List<Localization> _Items = new List<Localization>();
        public override List<Localization> Items
        {
            get
            {
                return _Items;
            }

            set
            {
                _Items = value;
                OnPropertyChanged();
            }
        }

        
        public bool HasNativeSupport()
        {
            foreach (GameLanguage gameLanguage in CheckLocalizations.PluginDatabase.PluginSettings.GameLanguages)
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
