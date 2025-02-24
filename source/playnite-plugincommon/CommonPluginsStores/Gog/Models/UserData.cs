using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace CommonPluginsStores.Gog.Models
{
    public class UserData
    {
        [SerializationPropertyName("country")]
        public string Country { get; set; }

        [SerializationPropertyName("currencies")]
        public List<Currency> Currencies { get; set; }

        [SerializationPropertyName("selectedCurrency")]
        public SelectedCurrency SelectedCurrency { get; set; }

        [SerializationPropertyName("preferredLanguage")]
        public PreferredLanguage PreferredLanguage { get; set; }

        [SerializationPropertyName("ratingBrand")]
        public string RatingBrand { get; set; }

        [SerializationPropertyName("isLoggedIn")]
        public bool IsLoggedIn { get; set; }

        [SerializationPropertyName("checksum")]
        public Checksum Checksum { get; set; }

        [SerializationPropertyName("updates")]
        public Updates Updates { get; set; }

        [SerializationPropertyName("userId")]
        public string UserId { get; set; }

        [SerializationPropertyName("username")]
        public string Username { get; set; }

        [SerializationPropertyName("galaxyUserId")]
        public string GalaxyUserId { get; set; }

        [SerializationPropertyName("email")]
        public string Email { get; set; }

        [SerializationPropertyName("avatar")]
        public string Avatar { get; set; }

        [SerializationPropertyName("walletBalance")]
        public WalletBalance WalletBalance { get; set; }

        [SerializationPropertyName("purchasedItems")]
        public PurchasedItems PurchasedItems { get; set; }

        [SerializationPropertyName("wishlistedItems")]
        public int WishlistedItems { get; set; }

        [SerializationPropertyName("friends")]
        public List<Friend> Friends { get; set; }

        [SerializationPropertyName("personalizedProductPrices")]
        public List<object> PersonalizedProductPrices { get; set; }

        [SerializationPropertyName("personalizedSeriesPrices")]
        public List<object> PersonalizedSeriesPrices { get; set; }
    }

    public class Currency
    {
        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("symbol")]
        public string Symbol { get; set; }
    }

    public class SelectedCurrency
    {
        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("symbol")]
        public string Symbol { get; set; }
    }

    public class PreferredLanguage
    {
        [SerializationPropertyName("code")]
        public string Code { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }

    public class Checksum
    {
        [SerializationPropertyName("cart")]
        public object Cart { get; set; }

        [SerializationPropertyName("games")]
        public string Games { get; set; }

        [SerializationPropertyName("wishlist")]
        public string Wishlist { get; set; }

        [SerializationPropertyName("reviews_votes")]
        public object ReviewsVotes { get; set; }

        [SerializationPropertyName("games_rating")]
        public object GamesRating { get; set; }
    }

    public class Updates
    {
        [SerializationPropertyName("messages")]
        public int Messages { get; set; }

        [SerializationPropertyName("pendingFriendRequests")]
        public int PendingFriendRequests { get; set; }

        [SerializationPropertyName("unreadChatMessages")]
        public int UnreadChatMessages { get; set; }

        [SerializationPropertyName("products")]
        public int Products { get; set; }

        [SerializationPropertyName("total")]
        public int Total { get; set; }
    }

    public class WalletBalance
    {
        [SerializationPropertyName("currency")]
        public string Currency { get; set; }

        [SerializationPropertyName("amount")]
        public int Amount { get; set; }
    }

    public class PurchasedItems
    {
        [SerializationPropertyName("games")]
        public int Games { get; set; }

        [SerializationPropertyName("movies")]
        public int Movies { get; set; }
    }

    public class Friend
    {
        [SerializationPropertyName("username")]
        public string Username { get; set; }

        [SerializationPropertyName("userSince")]
        public int UserSince { get; set; }

        [SerializationPropertyName("galaxyId")]
        public string GalaxyId { get; set; }

        [SerializationPropertyName("avatar")]
        public string Avatar { get; set; }
    }
}
