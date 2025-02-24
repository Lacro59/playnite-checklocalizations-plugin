using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonPluginsStores.Origin.Models
{
    public class ProductInfosResponse
    {
        public List<ProductInfo> productInfos { get; set; }
    }
    public class SoftwareList
    {
        public string softwarePlatform { get; set; }
        public string achievementSetOverride { get; set; }
    }

    public class Softwares
    {
        public List<SoftwareList> softwareList { get; set; }
    }

    public class ProductInfo
    {
        public string productId { get; set; }
        public string displayProductName { get; set; }
        public string cdnAssetRoot { get; set; }
        public string imageServer { get; set; }
        public object backgroundImage { get; set; }
        public string packArtSmall { get; set; }
        public string packArtMedium { get; set; }
        public string packArtLarge { get; set; }
        public Softwares softwares { get; set; }
        public string masterTitleId { get; set; }
        public string gameDistributionSubType { get; set; }
        public int gameEditionTypeFacetKeyRankDesc { get; set; }
    }

}
