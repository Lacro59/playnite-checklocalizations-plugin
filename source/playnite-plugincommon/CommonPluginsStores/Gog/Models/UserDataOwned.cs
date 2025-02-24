using Playnite.SDK.Data;
using System.Collections.Generic;

namespace CommonPluginsStores.Gog.Models
{
    public class UserDataOwned
    {
        [SerializationPropertyName("owned")]
        public List<int> Owned { get; set; }
    }
}
