using System.Collections.Generic;
using Playnite.SDK.Data;

namespace CommonPluginsStores.Steam.Models
{
    public class SteamUserData
    {
        [SerializationPropertyName("rgWishlist")]
        public List<int> RgWishlist { get; set; }

        [SerializationPropertyName("rgOwnedPackages")]
        public List<int> RgOwnedPackages { get; set; }

        [SerializationPropertyName("rgOwnedApps")]
        public List<int> RgOwnedApps { get; set; }

        [SerializationPropertyName("rgFollowedApps")]
        public List<int> RgFollowedApps { get; set; }

        [SerializationPropertyName("rgMasterSubApps")]
        public List<object> RgMasterSubApps { get; set; }

        [SerializationPropertyName("rgPackagesInCart")]
        public List<object> RgPackagesInCart { get; set; }

        [SerializationPropertyName("rgAppsInCart")]
        public List<object> RgAppsInCart { get; set; }

        [SerializationPropertyName("rgRecommendedTags")]
        public List<RgRecommendedTag> RgRecommendedTags { get; set; }

        [SerializationPropertyName("rgIgnoredApps")]
        public object RgIgnoredApps { get; set; }

        [SerializationPropertyName("rgIgnoredPackages")]
        public List<object> RgIgnoredPackages { get; set; }

        [SerializationPropertyName("rgHardwareUsed")]
        public List<string> RgHardwareUsed { get; set; }

        [SerializationPropertyName("rgCurators")]
        public object RgCurators { get; set; }

        [SerializationPropertyName("rgCuratorsIgnored")]
        public List<object> RgCuratorsIgnored { get; set; }

        [SerializationPropertyName("rgCurations")]
        public object RgCurations { get; set; }

        [SerializationPropertyName("bShowFilteredUserReviewScores")]
        public bool BShowFilteredUserReviewScores { get; set; }

        [SerializationPropertyName("rgCreatorsFollowed")]
        public List<int> RgCreatorsFollowed { get; set; }

        [SerializationPropertyName("rgCreatorsIgnored")]
        public List<object> RgCreatorsIgnored { get; set; }

        [SerializationPropertyName("rgExcludedTags")]
        public List<object> RgExcludedTags { get; set; }

        [SerializationPropertyName("rgExcludedContentDescriptorIDs")]
        public List<object> RgExcludedContentDescriptorIDs { get; set; }

        [SerializationPropertyName("rgAutoGrantApps")]
        public List<object> RgAutoGrantApps { get; set; }

        [SerializationPropertyName("rgRecommendedApps")]
        public List<int> RgRecommendedApps { get; set; }

        [SerializationPropertyName("rgPreferredPlatforms")]
        public List<string> RgPreferredPlatforms { get; set; }

        [SerializationPropertyName("rgPrimaryLanguage")]
        public int? RgPrimaryLanguage { get; set; }

        [SerializationPropertyName("rgSecondaryLanguages")]
        public List<int> RgSecondaryLanguages { get; set; }

        [SerializationPropertyName("bAllowAppImpressions")]
        public bool BAllowAppImpressions { get; set; }

        [SerializationPropertyName("nCartLineItemCount")]
        public int NCartLineItemCount { get; set; }

        [SerializationPropertyName("nRemainingCartDiscount")]
        public int NRemainingCartDiscount { get; set; }

        [SerializationPropertyName("nTotalCartDiscount")]
        public int NTotalCartDiscount { get; set; }
    }

    public class RgRecommendedTag
    {
        [SerializationPropertyName("tagid")]
        public int Tagid { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }
}
