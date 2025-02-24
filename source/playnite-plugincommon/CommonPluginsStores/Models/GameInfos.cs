using CommonPlayniteShared;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonPluginsStores.Models
{
    public class BasicGameInfos
    {
        public string Id { get; set; }
        public string Id2 { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Languages { get; set; }
        public DateTime? Released { get; set; }

        public string Image { get; set; }
        [DontSerialize]
        public string ImagePath => ImageSourceManagerPlugin.GetImagePath(Image);
    }

    public class GameInfos : BasicGameInfos
    {
        public ObservableCollection<DlcInfos> Dlcs { get; set; }
    }

    public class DlcInfos : BasicGameInfos
    {
        public bool IsOwned { get; set; }

        public string Price { get; set; }
        public string PriceBase { get; set; }

        [DontSerialize]
        public double PriceNumeric
        {
            get
            {
                if (Price.IsNullOrEmpty())
                {
                    return 0;
                }

                string temp = Price.Replace(",--", string.Empty).Replace(".--", string.Empty).Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + "--", string.Empty);
                temp = Regex.Split(temp, @"\s+").Where(s => s != string.Empty).First();
                temp = temp.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                temp = Regex.Replace(temp, @"[^\d" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + "-]", "");

                double.TryParse(temp, out double dPrice);
                return dPrice;
            }
        }

        [DontSerialize]
        public double PriceBaseNumeric
        {
            get
            {
                if (PriceBase.IsNullOrEmpty())
                {
                    return 0;
                }

                string temp = PriceBase.Replace(",--", string.Empty).Replace(".--", string.Empty).Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + "--", string.Empty);
                temp = Regex.Split(temp, @"\s+").Where(s => s != string.Empty).First();
                temp = temp.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                temp = Regex.Replace(temp, @"[^\d" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + "-]", "");

                double.TryParse(temp, out double dPrice);
                return dPrice;
            }
        }

        [DontSerialize]
        public bool IsFree
        {
            get
            {
                try
                {
                    if (Price.IsNullOrEmpty())
                    {
                        return false;
                    }
                    return PriceNumeric == 0;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
                    return false;
                }
            }
        }

        [DontSerialize]
        public bool IsDiscount
        {
            get
            {
                return !Price.IsEqual(PriceBase);
            }
        }
    }
}
