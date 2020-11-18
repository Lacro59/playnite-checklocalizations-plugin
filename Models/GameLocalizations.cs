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
        private List<Localization> _Data = new List<Localization>();
        public override List<Localization> Data
        {
            get
            {
                return _Data;
            }

            set
            {
                _Data = value;
                OnPropertyChanged();
            }
        }
    }
}
