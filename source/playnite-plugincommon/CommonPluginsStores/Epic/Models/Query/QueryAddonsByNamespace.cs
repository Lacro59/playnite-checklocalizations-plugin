using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models.Query
{
    public class QueryAddonsByNamespace
    {
        public class Variables
        {
            public string locale = "en-US";
            public string country = "US";
            public string epic_namespace = "";
            public string sortBy = "releaseDate";
            public string sortDir = "asc";
            public string categories = "addons|digitalextras";
            public int count = 50;
        }

        public Variables variables = new Variables();
        public string query = @"query getAddonsByNamespace($categories: String!, $count: Int!, $country: String!, $locale: String!, $epic_namespace: String!, $sortBy: String!, $sortDir: String!) {    Catalog {        catalogOffers(namespace: $epic_namespace, locale: $locale, params: {            category: $categories,            count: $count,            country: $country,            sortBy: $sortBy,            sortDir: $sortDir        }) {            elements {                countriesBlacklist                customAttributes {                    key                    value                }                description                developer                effectiveDate                id                isFeatured                keyImages {                    type                    url                }                lastModifiedDate                longDescription                namespace                offerType                productSlug                releaseDate                status                technicalDetails                title                urlSlug                price(country: $country) {                    totalPrice {                        discountPrice                        originalPrice                        voucherDiscount                        discount                        currencyCode                        currencyInfo {                            decimals                        }                        fmtPrice(locale: $locale) {                            originalPrice                            discountPrice                            intermediatePrice                        }                    }                }            }        }    }}";
    }
}
