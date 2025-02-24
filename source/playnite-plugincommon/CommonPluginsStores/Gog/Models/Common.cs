using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Gog.Models
{
    public class Links
    {
        [SerializationPropertyName("self")]
        public Ref Self { get; set; }

        [SerializationPropertyName("first")]
        public Ref First { get; set; }

        [SerializationPropertyName("last")]
        public Ref Last { get; set; }

        [SerializationPropertyName("next")]
        public Ref Next { get; set; }
    }

    public class Ref
    {
        [SerializationPropertyName("href")]
        public string Href { get; set; }
    }
}
