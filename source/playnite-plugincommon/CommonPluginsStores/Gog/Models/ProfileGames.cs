using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace CommonPluginsStores.Gog.Models
{
    public class ProfileGames
    {
        [SerializationPropertyName("page")]
        public int Page { get; set; }

        [SerializationPropertyName("limit")]
        public int Limit { get; set; }

        [SerializationPropertyName("pages")]
        public int Pages { get; set; }

        [SerializationPropertyName("total")]
        public int Total { get; set; }

        [SerializationPropertyName("_links")]
        public Links Links { get; set; }

        [SerializationPropertyName("_embedded")]
        public Embedded Embedded { get; set; }
    }

    public class Game
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("achievementSupport")]
        public bool AchievementSupport { get; set; }

        [SerializationPropertyName("image")]
        public string Image { get; set; }
    }

    public class GameItem
    {
        [SerializationPropertyName("game")]
        public Game Game { get; set; }

        [SerializationPropertyName("stats")]
        public object Stats { get; set; }
    }

    public class Embedded
    {
        [SerializationPropertyName("items")]
        public List<GameItem> Items { get; set; }
    }
}
