using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Gog.Models
{
    public class AccountBasicResponse
    {
        [SerializationPropertyName("isLoggedIn")]
        public bool IsLoggedIn { get; set; }

        [SerializationPropertyName("userId")]
        public string UserId { get; set; }

        [SerializationPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [SerializationPropertyName("accessTokenExpires")]
        public int AccessTokenExpires { get; set; }

        [SerializationPropertyName("clientId")]
        public string ClientId { get; set; }

        [SerializationPropertyName("username")]
        public string Username { get; set; }

        [SerializationPropertyName("avatars")]
        public Avatars Avatars { get; set; }

        [SerializationPropertyName("avatar")]
        public string Avatar { get; set; }

        [SerializationPropertyName("profileState")]
        public string ProfileState { get; set; }

        [SerializationPropertyName("cacheExpires")]
        public int CacheExpires { get; set; }
    }

    public class Avatars
    {
        [SerializationPropertyName("menu_user_av_small")]
        public string MenuUserAvSmall { get; set; }

        [SerializationPropertyName("menu_user_av_small2")]
        public string MenuUserAvSmall2 { get; set; }

        [SerializationPropertyName("menu_user_av_big")]
        public string MenuUserAvBig { get; set; }

        [SerializationPropertyName("menu_user_av_big2")]
        public string MenuUserAvBig2 { get; set; }
    }
}
