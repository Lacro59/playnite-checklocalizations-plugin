using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    public class AccountInfoResponse
    {
        public Pid pid { get; set; }
    }

    public class Pid
    {
        public string externalRefType { get; set; }
        public string externalRefValue { get; set; }
        public string pidId { get; set; }
        //public string email { get; set; }
        public string emailStatus { get; set; }
        public string strength { get; set; }
        public string dob { get; set; }
        public string country { get; set; }
        public string language { get; set; }
        public string locale { get; set; }
        public string status { get; set; }
        public string reasonCode { get; set; }
        public string tosVersion { get; set; }
        public string parentalEmail { get; set; }
        public string thirdPartyOptin { get; set; }
        public string globalOptin { get; set; }
        public string dateCreated { get; set; }
        public string dateModified { get; set; }
        public string lastAuthDate { get; set; }
        public string registrationSource { get; set; }
        public string authenticationSource { get; set; }
        public string showEmail { get; set; }
        public string discoverableEmail { get; set; }
        public string anonymousPid { get; set; }
        public string underagePid { get; set; }
        public string defaultBillingAddressUri { get; set; }
        public string defaultShippingAddressUri { get; set; }
        //public int passwordSignature { get; set; }
    }
}
