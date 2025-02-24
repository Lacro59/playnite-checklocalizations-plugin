using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Xbox.Models
{
    public class Disclaimer
    {
        public bool hasDisclaimer { get; set; }
        public string text { get; set; }
    }

    public class Image
    {
        public double width { get; set; }
        public double height { get; set; }
        public string baseUri { get; set; }
        public string system { get; set; }
        public string alt { get; set; }
        public string background { get; set; }
        public string imagePosition { get; set; }
        public string purpose { get; set; }
    }

    public class Price
    {
        public string taxLabel { get; set; }
        public Disclaimer disclaimer { get; set; }
        public bool hasAddOns { get; set; }
        public string priceFormat { get; set; }
        public string currency { get; set; }
        public string currencySymbol { get; set; }
        public string currentPrice { get; set; }
        public double currentValue { get; set; }
        public string fromText { get; set; }
    }

    public class Product
    {
        public string id { get; set; }
        public string locale { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string pdpUri { get; set; }
        public Price price { get; set; }
        public Image image { get; set; }
        public string productFamily { get; set; }
        public string skuId { get; set; }
    }

    public class Wishlists
    {
        public string name { get; set; }
        public List<Product> products { get; set; }
        public bool hasUnavailableProducts { get; set; }
        public Settings settings { get; set; }
        public string id { get; set; }
    }

    public class Settings
    {
        public bool notificationOptIn { get; set; }
        public string visibility { get; set; }
    }


}
