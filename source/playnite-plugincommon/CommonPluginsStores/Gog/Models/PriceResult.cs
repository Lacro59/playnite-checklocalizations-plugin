using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace CommonPluginsStores.Gog.Models
{
    public class PriceResult
    {
        [SerializationPropertyName("items")]
        public List<PriceItem> Items { get; set; }
    }

    public class PriceCurrency
    {
        [SerializationPropertyName("code")]
        public string Code { get; set; }
    }

    public class Price
    {
        [SerializationPropertyName("currency")]
        public PriceCurrency Currency { get; set; }

        [SerializationPropertyName("basePrice")]
        public string BasePrice { get; set; }

        [SerializationPropertyName("finalPrice")]
        public string FinalPrice { get; set; }

        [SerializationPropertyName("bonusWalletFunds")]
        public string BonusWalletFunds { get; set; }
    }

    public class PriceProduct
    {
        [SerializationPropertyName("id")]
        public int Id { get; set; }
    }

    public class PriceEmbedded
    {
        [SerializationPropertyName("prices")]
        public List<Price> Prices { get; set; }

        [SerializationPropertyName("product")]
        public PriceProduct Product { get; set; }
    }

    public class PriceItem
    {
        [SerializationPropertyName("_links")]
        public Links Links { get; set; }

        [SerializationPropertyName("_embedded")]
        public PriceEmbedded Embedded { get; set; }
    }
}
