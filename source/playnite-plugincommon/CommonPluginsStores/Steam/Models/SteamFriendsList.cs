using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamFriendsList
    {
        [SerializationPropertyName("friendslist")]
        public FriendsList FriendsList { get; set; }
    }

    public class Friend
    {
        [SerializationPropertyName("steamid")]
        public ulong SteamId { get; set; }
        [SerializationPropertyName("relationship")]
        public string Relationship { get; set; }
        [SerializationPropertyName("friend_since")]
        public int FriendSince { get; set; }
    }

    public class FriendsList
    {
        [SerializationPropertyName("friends")]
        public List<Friend> Friends { get; set; }
    }
}
