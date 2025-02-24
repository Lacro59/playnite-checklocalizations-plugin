using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Steam.Models
{
    public class RgProfileData
    {
        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("steamid")]
        public ulong SteamId { get; set; }

        [SerializationPropertyName("personaname")]
        public string PersonaName { get; set; }

        [SerializationPropertyName("summary")]
        public string Summary { get; set; }
    }
}
