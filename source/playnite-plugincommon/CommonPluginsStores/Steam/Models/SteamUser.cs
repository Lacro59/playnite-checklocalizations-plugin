using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamUser
    {
        public ulong SteamId { get; set; }
        public uint AccountId => SteamApi.GetAccountId(SteamId);
        public string AccountName { get; set; }
        public string PersonaName { get; set; }
    }
}
