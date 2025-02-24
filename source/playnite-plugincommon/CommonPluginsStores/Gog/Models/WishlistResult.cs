using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Gog.Models
{
    public class WishlistResult
    {
        [SerializationPropertyName("wishlist")]
        public dynamic Wishlist { get; set; }

        [SerializationPropertyName("checksum")]
        public string Checksum { get; set; }
    }
}
