using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models
{
    public class EpicWishlistData
    {
        public WishlistData data { get; set; }
        public Extensions extensions { get; set; }
    }

    public class Offer
    {
        public string id { get; set; }
        public string title { get; set; }
        public string productSlug { get; set; }
        public string urlSlug { get; set; }
        public string offerType { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public string status { get; set; }
        public bool isCodeRedemptionOnly { get; set; }
        public List<KeyImage> keyImages { get; set; }
        public WishlistCatalogNs catalogNs { get; set; }
        public List<OfferMapping> offerMappings { get; set; }
    }

    public class WishlistCatalogNs
    {
        public List<Mapping> mappings { get; set; }
    }

    public class Mapping
    {
        public string pageSlug { get; set; }
        public string pageType { get; set; }
    }

    public class WishlistElement
    {
        public string id { get; set; }
        public object order { get; set; }
        public DateTime created { get; set; }
        public string offerId { get; set; }
        public DateTime updated { get; set; }
        public string @namespace { get; set; }
        public Offer offer { get; set; }
    }

    public class WishlistItems
    {
        public List<WishlistElement> elements { get; set; }
    }

    public class EpicWishlistItems
    {
        public WishlistItems wishlistItems { get; set; }
    }

    public class WishlistData
    {
        public EpicWishlistItems Wishlist { get; set; }
    }
}
