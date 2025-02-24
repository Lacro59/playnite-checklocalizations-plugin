using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamApps
    {
        [SerializationPropertyName("applist")]
        public Applist AppList { get; set; }
    }

    public class App
    {
        [SerializationPropertyName("appid")]
        public int AppId { get; set; }
        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public class Applist
    {
        [SerializationPropertyName("apps")]
        public List<App> Apps { get; set; }
    }
}
