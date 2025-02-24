using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace CommonPluginsStores.Gog.Models
{
    public class ProfileUser
    {
        [SerializationPropertyName("username")]
        public string Username { get; set; }

        [SerializationPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        [SerializationPropertyName("userId")]
        public string UserId { get; set; }

        [SerializationPropertyName("avatar")]
        public string Avatar { get; set; }

        [SerializationPropertyName("settings")]
        public Settings Settings { get; set; }

        [SerializationPropertyName("stats")]
        public Stats Stats { get; set; }

        [SerializationPropertyName("background")]
        public Background Background { get; set; }
    }

    public class Settings
    {
        [SerializationPropertyName("allow_to_be_invited_by")]
        public string AllowToBeInvitedBy { get; set; }
    }

    public class Stats
    {
        [SerializationPropertyName("games_owned")]
        public int GamesOwned { get; set; }

        [SerializationPropertyName("achievements")]
        public int? Achievements { get; set; }

        [SerializationPropertyName("hours_played")]
        public int HoursPlayed { get; set; }
    }

    public class Background
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }

        [SerializationPropertyName("src")]
        public string Src { get; set; }

        [SerializationPropertyName("background_dominant_color")]
        public List<int> BackgroundDominantColor { get; set; }
    }
}
