using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLocalizations.Models
{
    public class GameLanguage
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsTag { get; set; }
        public bool IsNative { get; set; }
    }
}
