using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models.SteamKit
{
    public class SteamOwnedGame
    {
        public int Appid { get; set; }
        public int PlaytimeForever { get; set; }
        public int Playtime2weeks { get; set; }
        public int PlaytimeWindowsForever { get; set; }
        public int PlaytimeMacForever { get; set; }
        public int PlaytimeLinuxForever { get; set; }
        public int PlaytimeDeckForever { get; set; }
        public DateTime RtimeLastPlayed { get; set; }
        public int PlaytimeDisconnected { get; set; }
    }
}
