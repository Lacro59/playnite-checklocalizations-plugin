using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    public class PriceResult
    {
        public List<Offer> offer { get; set; }
    }

    public class Promotion
    {
        public string promotionRuleId { get; set; }
        public double discountAmount { get; set; }
        public double discountRate { get; set; }
        public object couponCode { get; set; }
        public string promotionRuleType { get; set; }
        public int usage { get; set; }
    }

    public class Promotions
    {
        public List<Promotion> promotion { get; set; }
    }

    public class RecommendedPromotions
    {
        public List<object> promotion { get; set; }
    }

    public class Rating
    {
        public object originalConfiguredPrice { get; set; }
        public double originalUnitPrice { get; set; }
        public int quantity { get; set; }
        public double originalTotalPrice { get; set; }
        public double finalTotalAmount { get; set; }
        public double totalDiscountAmount { get; set; }
        public double totalDiscountRate { get; set; }
        public Promotions promotions { get; set; }
        public RecommendedPromotions recommendedPromotions { get; set; }
        public string currency { get; set; }
    }

    public class Offer
    {
        public string offerId { get; set; }
        public string offerType { get; set; }
        public List<Rating> rating { get; set; }
        public object error { get; set; }
    }
}
