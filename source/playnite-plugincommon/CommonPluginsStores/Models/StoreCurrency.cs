using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Models
{
    public class StoreCurrency
    {
        public string country { get; set; }
        public string currency { get; set; }
        public string symbol { get; set; }

        [DontSerialize]
        public string text
        {
            get
            {
                string textFormated = string.Empty;
                if (!country.IsNullOrEmpty())
                {
                    textFormated += country.ToUpper();
                }
                if (!currency.IsNullOrEmpty())
                {
                    if (!textFormated.IsNullOrEmpty())
                    {
                        textFormated += " - ";
                    }
                    textFormated += currency.ToUpper();
                }
                if (!symbol.IsNullOrEmpty())
                {
                    if (!textFormated.IsNullOrEmpty())
                    {
                        textFormated += " - ";
                    }
                    textFormated += symbol;
                }
                return textFormated;
             }
        }
    }
}
