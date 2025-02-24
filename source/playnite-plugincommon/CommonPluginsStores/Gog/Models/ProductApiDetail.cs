using System;
using System.Collections.Generic;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Gog.Models
{
    public class GogDlcs
    {
        [SerializationPropertyName("products")]
        public List<Product> Products { get; set; }

        [SerializationPropertyName("all_products_url")]
        public string AllProductsUrl { get; set; }

        [SerializationPropertyName("expanded_all_products_url")]
        public string ExpandedAllProductsUrl { get; set; }
    }

    public class Product
    {
        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("link")]
        public string Link { get; set; }

        [SerializationPropertyName("expanded_link")]
        public string ExpandedLink { get; set; }
    }

    public class ProductApiDetail
    {
        public class Compatibility
        {
            [SerializationPropertyName("windows")]
            public bool Windows { get; set; }

            [SerializationPropertyName("osx")]
            public bool Osx { get; set; }

            [SerializationPropertyName("linux")]
            public bool Linux { get; set; }
        }

        public class Links
        {
            [SerializationPropertyName("purchase_link")]
            public string PurchaseLink { get; set; }

            [SerializationPropertyName("product_card")]
            public string ProductCard { get; set; }

            [SerializationPropertyName("support")]
            public string Support { get; set; }

            [SerializationPropertyName("forum")]
            public string Forum { get; set; }
        }

        public class Images
        {
            [SerializationPropertyName("background")]
            public string Background { get; set; }

            [SerializationPropertyName("logo")]
            public string Logo { get; set; }

            [SerializationPropertyName("logo2x")]
            public string Logo2x { get; set; }

            [SerializationPropertyName("icon")]
            public string Icon { get; set; }

            [SerializationPropertyName("sidebarIcon")]
            public string SidebarIcon { get; set; }

            [SerializationPropertyName("sidebarIcon2x")]
            public string SidebarIcon2x { get; set; }
        }

        public class Description
        {
            [SerializationPropertyName("lead")]
            public string Lead { get; set; }

            [SerializationPropertyName("full")]
            public string Full { get; set; }

            [SerializationPropertyName("whats_cool_about_it")]
            public string WhatsCoolAboutIt { get; set; }
        }

        [SerializationPropertyName("id")]
        public int Id { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("slug")]
        public string Slug { get; set; }

        [SerializationPropertyName("content_system_compatibility")]
        public Compatibility ContentSystemCompatibility { get; set; }

        [SerializationPropertyName("languages")]
        public object Languages { get; set; }

        [SerializationPropertyName("links")]
        public Links ProductLinks { get; set; }

        [SerializationPropertyName("is_secret")]
        public bool IsSecret { get; set; }

        [SerializationPropertyName("game_type")]
        public string GameType { get; set; }

        [SerializationPropertyName("is_pre_order")]
        public bool IsPreOrder { get; set; }

        [SerializationPropertyName("images")]
        public Images ProductImages { get; set; }

        [SerializationPropertyName("description")]
        public Description ProductDescription { get; set; }

        [SerializationPropertyName("release_date")]
        public DateTime? ReleaseDate { get; set; }

        [SerializationPropertyName("dlcs")]
        public dynamic Dlcs { get; set; }
    }
}
