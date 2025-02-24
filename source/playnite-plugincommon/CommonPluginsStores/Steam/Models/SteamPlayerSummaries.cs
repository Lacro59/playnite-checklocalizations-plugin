using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamPlayerSummaries
    {
        [SerializationPropertyName("response")]
        public PlayerResponse Response { get; set; }
    }

    public class Player
    {
        [SerializationPropertyName("steamid")]
        public ulong SteamId { get; set; }
        [SerializationPropertyName("communityvisibilitystate")]
        public int CommunityVisibilityState { get; set; }
        [SerializationPropertyName("profilestate")]
        public int ProfileState { get; set; }
        [SerializationPropertyName("personaname")]
        public string PersonaName { get; set; }
        [SerializationPropertyName("profileurl")]
        public string ProfileUrl { get; set; }
        [SerializationPropertyName("avatar")]
        public string Avatar { get; set; }
        [SerializationPropertyName("avatarmedium")]
        public string AvatarMedium { get; set; }
        [SerializationPropertyName("avatarfull")]
        public string AvatarFull { get; set; }
        [SerializationPropertyName("avatarhash")]
        public string AvatarHash { get; set; }
        [SerializationPropertyName("lastlogoff")]
        public int LastLogOff { get; set; }
        [SerializationPropertyName("personastate")]
        public int PersonaState { get; set; }
        [SerializationPropertyName("primaryclanid")]
        public string PrimaryClanId { get; set; }
        [SerializationPropertyName("timecreated")]
        public int TimeCreated { get; set; }
        [SerializationPropertyName("personastateflags")]
        public int PersonaStateFlags { get; set; }
        [SerializationPropertyName("loccountrycode")]
        public string LocCountryCode { get; set; }
    }

    public class PlayerResponse
    {
        [SerializationPropertyName("players")]
        public List<Player> Players { get; set; }
    }
}
