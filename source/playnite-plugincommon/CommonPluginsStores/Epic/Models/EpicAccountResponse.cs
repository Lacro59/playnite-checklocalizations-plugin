using System;
using System.Collections.Generic;
using System.Text;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Epic.Models
{
    public class EpicAccountResponse
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("displayName")]
        public string DisplayName { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("email")]
        public string Email { get; set; }

        [SerializationPropertyName("failedLoginAttempts")]
        public int FailedLoginAttempts { get; set; }

        [SerializationPropertyName("lastLogin")]
        public DateTime LastLogin { get; set; }

        [SerializationPropertyName("numberOfDisplayNameChanges")]
        public int NumberOfDisplayNameChanges { get; set; }

        [SerializationPropertyName("ageGroup")]
        public string AgeGroup { get; set; }

        [SerializationPropertyName("headless")]
        public bool Headless { get; set; }

        [SerializationPropertyName("country")]
        public string Country { get; set; }

        [SerializationPropertyName("lastName")]
        public string LastName { get; set; }

        [SerializationPropertyName("preferredLanguage")]
        public string PreferredLanguage { get; set; }

        [SerializationPropertyName("canUpdateDisplayName")]
        public bool CanUpdateDisplayName { get; set; }

        [SerializationPropertyName("tfaEnabled")]
        public bool TfaEnabled { get; set; }

        [SerializationPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [SerializationPropertyName("minorVerified")]
        public bool MinorVerified { get; set; }

        [SerializationPropertyName("minorExpected")]
        public bool MinorExpected { get; set; }

        [SerializationPropertyName("minorStatus")]
        public string MinorStatus { get; set; }

        [SerializationPropertyName("siweNotificationEnabled")]
        public bool SiweNotificationEnabled { get; set; }

        [SerializationPropertyName("cabinedMode")]
        public bool CabinedMode { get; set; }

        [SerializationPropertyName("hasHashedEmail")]
        public bool HasHashedEmail { get; set; }

        [SerializationPropertyName("lastReviewedSecuritySettings")]
        public DateTime LastReviewedSecuritySettings { get; set; }
    }


}
