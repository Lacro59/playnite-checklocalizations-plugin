using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    public class GameStoreDataResponse
    {
        public string offerId { get; set; }
        public string offerType { get; set; }
        public List<string> extraContent { get; set; }
        public List<string> displayTypeNoneExtraContent { get; set; }
        public string isDownloadable { get; set; }
        public string gameDistributionSubType { get; set; }
        public object trialLaunchDuration { get; set; }
        public string masterTitleId { get; set; }
        public List<object> alternateMasterTitleIds { get; set; }
        public List<string> suppressedOfferIds { get; set; }
        public string originDisplayType { get; set; }
        public List<Platform> platforms { get; set; }
        public string ratingSystemIcon { get; set; }
        public string gameRatingUrl { get; set; }
        public object gameRatingPendingMature { get; set; }
        public object gameRatingDescriptionLong { get; set; }
        public string gameRatingTypeValue { get; set; }
        public string gameRatingReason { get; set; }
        public List<object> gameRatingInteractiveElements { get; set; }
        public List<object> gameRatingDesc { get; set; }
        public string franchiseFacetKey { get; set; }
        public string mdmItemType { get; set; }
        public string platformFacetKey { get; set; }
        public string publisherFacetKey { get; set; }
        public string imageServer { get; set; }
        public string developerFacetKey { get; set; }
        public string revenueModel { get; set; }
        public List<string> softwareLocales { get; set; }
        public string gameTypeFacetKey { get; set; }
        public string masterTitle { get; set; }
        public string gameEditionTypeFacetKey { get; set; }
        public string numberOfPlayersFacetKey { get; set; }
        public string genreFacetKey { get; set; }
        public string itemName { get; set; }
        public string itemType { get; set; }
        public string itemId { get; set; }
        public string rbuCode { get; set; }
        public string brand { get; set; }
        public string storeGroupId { get; set; }
        public string dynamicPricing { get; set; }
        public Countries countries { get; set; }
        public I18n i18n { get; set; }
        public Vault vault { get; set; }
        public List<object> includeOffers { get; set; }
        public string contentId { get; set; }
        public string gameNameFacetKey { get; set; }
        public string gameEditionTypeFacetKeyRankDesc { get; set; }
        public string offerPath { get; set; }
        public object extraContentDisplayGroup { get; set; }
        public object extraContentDisplayGroupSortAsc { get; set; }
        public string gameRatingType { get; set; }
        public List<object> bundleOffers { get; set; }
        public object duration { get; set; }
        public object durationUnit { get; set; }
        public object firstCycleExtraTime { get; set; }
        public object firstCycleExtraTimeUnit { get; set; }
        public object isThirdPartyTitle { get; set; }
        public string isShellBundle { get; set; }
        public string projectNumber { get; set; }
        public DateTime? storeStartDate { get; set; }
        public object storeEndDate { get; set; }
        public object isRepurchaseable { get; set; }
        public PremiumVault premiumVault { get; set; }
        public List<FirstParty> firstParties { get; set; }
        public object forcePartnerEntitlementPending { get; set; }
        public string gdpPath { get; set; }
        public object softDependency { get; set; }
        public string isZeroPricedOffer { get; set; }
        public bool allowDigitalRefund { get; set; }
        public object digitalItoSupport { get; set; }
        public object digitalItoInstaller { get; set; }
        public object loyaltyPromotion { get; set; }
    }

    public class Platform
    {
        public string platform { get; set; }
        public string multiPlayerId { get; set; }
        public string downloadPackageType { get; set; }
        public DateTime? releaseDate { get; set; }
        public DateTime? downloadStartDate { get; set; }
        public object useEndDate { get; set; }
        public object executePathOverride { get; set; }
        public object achievementSetOverride { get; set; }
        public string showSubsSaveGameWarning { get; set; }
        public object commerceProfile { get; set; }
        public object originSubscriptionUseEndDate { get; set; }
        public object originSubscriptionUnlockDate { get; set; }
    }

    public class Countries
    {
        public string isPurchasable { get; set; }
        public object inStock { get; set; }
        public double? catalogPrice { get; set; }
        public string countryCurrency { get; set; }
        public List<double> catalogPriceA { get; set; }
        public List<string> countryCurrencyA { get; set; }
        public string hasSubscriberDiscount { get; set; }
        public bool isPublished { get; set; }
        public bool isAvailableForPreview { get; set; }
        public string giftable { get; set; }
        public List<object> hideStoreChannels { get; set; }
        public List<object> bannedCountries { get; set; }
        public DateTime? billingDate { get; set; }
        public object cycleFee { get; set; }
        public object purchaseFee { get; set; }
    }

    public class I18n
    {
        public object franchiseFacetKey { get; set; }
        public string systemRequirements { get; set; }
        public object platformFacetKey { get; set; }
        public string longDescription { get; set; }
        public string officialSiteURL { get; set; }
        public object publisherFacetKey { get; set; }
        public object developerFacetKey { get; set; }
        public string shortDescription { get; set; }
        public string onlineDisclaimer { get; set; }
        public string eulaURL { get; set; }
        public string gameForumURL { get; set; }
        public object gameTypeFacetKey { get; set; }
        public object numberOfPlayersFacetKey { get; set; }
        public object genreFacetKey { get; set; }
        public object franchisePageLink { get; set; }
        public object brand { get; set; }
        public string displayName { get; set; }
        public object preAnnouncementDisplayDate { get; set; }
        public object ratingSystemIcon { get; set; }
        public string packArtSmall { get; set; }
        public string packArtMedium { get; set; }
        public string packArtLarge { get; set; }
        public object gameManualURL { get; set; }
        public object extraContentDisplayGroupDisplayName { get; set; }
        public object mediumDescription { get; set; }
    }

    public class Vault
    {
        public string path { get; set; }
        public string isUpgradeable { get; set; }
        public object vaultStartDate { get; set; }
        public object vaultEndDate { get; set; }
    }

    public class PremiumVault
    {
        public string path { get; set; }
        public string isUpgradeable { get; set; }
        public object vaultStartDate { get; set; }
        public object vaultEndDate { get; set; }
    }

    public class FirstParty
    {
        public string partner { get; set; }
        public string partnerIdType { get; set; }
        public string partnerIdFlag { get; set; }
        public string partnerId { get; set; }
    }
}
