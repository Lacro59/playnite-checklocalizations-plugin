using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    public class PriceData
    {
        public PriceResult Price { get; set; }
        public string CodeCurrency { get; set; }
        public string SymbolCurrency { get; set; }
    }
}
