using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    class Wishlists
    {
        public List<Wishlist> wishlist { get; set; }
    }

    public class Wishlist
    {
        public string offerId { get; set; }
        public int displayOrder { get; set; }
        public object addedAt { get; set; }
    }
}
