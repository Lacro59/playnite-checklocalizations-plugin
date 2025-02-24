using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamWishlist
    {
        [SerializationPropertyName("mutations")]
        public List<object> Mutations { get; set; }

        [SerializationPropertyName("queries")]
        public List<Query> Queries { get; set; }
    }

    public class Query
    {
        [SerializationPropertyName("state")]
        public State State { get; set; }

        [SerializationPropertyName("queryKey")]
        public List<object> QueryKey { get; set; }

        [SerializationPropertyName("queryHash")]
        public string QueryHash { get; set; }
    }

    public class State
    {
        [SerializationPropertyName("data")]
        public object Data { get; set; }

        [SerializationPropertyName("dataUpdateCount")]
        public int? DataUpdateCount { get; set; }

        [SerializationPropertyName("dataUpdatedAt")]
        public long? DataUpdatedAt { get; set; }

        [SerializationPropertyName("error")]
        public object Error { get; set; }

        [SerializationPropertyName("errorUpdateCount")]
        public int ErrorUpdateCount { get; set; }

        [SerializationPropertyName("errorUpdatedAt")]
        public int ErrorUpdatedAt { get; set; }

        [SerializationPropertyName("fetchFailureCount")]
        public int FetchFailureCount { get; set; }

        [SerializationPropertyName("fetchFailureReason")]
        public object FetchFailureReason { get; set; }

        [SerializationPropertyName("fetchMeta")]
        public object FetchMeta { get; set; }

        [SerializationPropertyName("isInvalidated")]
        public bool IsInvalidated { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }

        [SerializationPropertyName("fetchStatus")]
        public string FetchStatus { get; set; }
    }



    public class Item
    {
        [SerializationPropertyName("appid")]
        public int Appid { get; set; }

        [SerializationPropertyName("priority")]
        public int Priority { get; set; }

        [SerializationPropertyName("date_added")]
        public int DateAdded { get; set; }
    }

    public class Apps
    {
        [SerializationPropertyName("steamid")]
        public string Steamid { get; set; }

        [SerializationPropertyName("items")]
        public List<Item> Items { get; set; }
    }


    public class BestPurchaseOption
    {
        [SerializationPropertyName("packageid")]
        public int Packageid { get; set; }

        [SerializationPropertyName("purchase_option_name")]
        public string PurchaseOptionName { get; set; }

        [SerializationPropertyName("final_price_in_cents")]
        public string FinalPriceInCents { get; set; }

        [SerializationPropertyName("formatted_final_price")]
        public string FormattedFinalPrice { get; set; }

        [SerializationPropertyName("active_discounts")]
        public List<object> ActiveDiscounts { get; set; }

        [SerializationPropertyName("user_can_purchase_as_gift")]
        public bool UserCanPurchaseAsGift { get; set; }

        [SerializationPropertyName("hide_discount_pct_for_compliance")]
        public bool HideDiscountPctForCompliance { get; set; }

        [SerializationPropertyName("included_game_count")]
        public int IncludedGameCount { get; set; }
    }

    public class Wishlist
    {
        [SerializationPropertyName("item_type")]
        public int ItemType { get; set; }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("success")]
        public int Success { get; set; }

        [SerializationPropertyName("visible")]
        public bool Visible { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("store_url_path")]
        public string StoreUrlPath { get; set; }

        [SerializationPropertyName("appid")]
        public int Appid { get; set; }

        [SerializationPropertyName("type")]
        public int Type { get; set; }

        [SerializationPropertyName("included_types")]
        public List<object> IncludedTypes { get; set; }

        [SerializationPropertyName("included_appids")]
        public List<object> IncludedAppids { get; set; }

        [SerializationPropertyName("content_descriptorids")]
        public List<object> ContentDescriptorids { get; set; }

        [SerializationPropertyName("best_purchase_option")]
        public BestPurchaseOption BestPurchaseOption { get; set; }
    }


    public class Response
    {
        [SerializationPropertyName("items")]
        public List<Item> Items { get; set; }
    }

    public class SteamWishlistApi
    {
        [SerializationPropertyName("response")]
        public Response Response { get; set; }
    }
}
