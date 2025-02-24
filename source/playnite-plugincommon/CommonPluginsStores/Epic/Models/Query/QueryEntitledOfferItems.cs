using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Epic.Models.Query
{
    public class QueryEntitledOfferItems
    {
        public class Variables
        {
            public string productNameSpace = "";
            public string offerId = "";
        }

        public Variables variables = new Variables();
        public string query = @"query getEntitledOfferItems($productNameSpace: String!, $offerId: String!) {    Launcher {        entitledOfferItems(namespace: $productNameSpace, offerId: $offerId) {            namespace            offerId            entitledToAllItemsInOffer            entitledToAnyItemInOffer        }    }}";
    }
}
