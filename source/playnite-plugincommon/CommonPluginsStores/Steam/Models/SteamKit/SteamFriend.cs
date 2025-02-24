using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models.SteamKit
{
    public class SteamFriend
    {
        public ulong SteamId { get; set; }
        public string Relationship { get; set; }
        public DateTime FriendSince { get; set; }
    }
}
