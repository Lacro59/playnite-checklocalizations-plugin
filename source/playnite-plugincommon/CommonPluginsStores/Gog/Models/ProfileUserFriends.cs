using Playnite.SDK.Data;
using System;
using System.Collections.Generic;

namespace CommonPluginsStores.Gog.Models
{
    public class ProfileUserFriends
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("user")]
        public User User { get; set; }

        [SerializationPropertyName("status")]
        public int Status { get; set; }

        [SerializationPropertyName("date_created")]
        public DateCreated DateCreated { get; set; }

        [SerializationPropertyName("date_accepted")]
        public DateAccepted DateAccepted { get; set; }

        [SerializationPropertyName("stats")]
        public Stats Stats { get; set; }
    }

    public class User
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("username")]
        public string Username { get; set; }

        [SerializationPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        [SerializationPropertyName("avatar")]
        public string Avatar { get; set; }

        [SerializationPropertyName("is_employee")]
        public bool IsEmployee { get; set; }

        [SerializationPropertyName("tags")]
        public List<object> Tags { get; set; }
    }

    public class DateCreated
    {
        [SerializationPropertyName("date")]
        public string Date { get; set; }

        [SerializationPropertyName("timezone_type")]
        public int TimezoneType { get; set; }

        [SerializationPropertyName("timezone")]
        public string Timezone { get; set; }
    }

    public class DateAccepted
    {
        [SerializationPropertyName("date")]
        public string Date { get; set; }

        [SerializationPropertyName("timezone_type")]
        public int TimezoneType { get; set; }

        [SerializationPropertyName("timezone")]
        public string Timezone { get; set; }
    }
}
