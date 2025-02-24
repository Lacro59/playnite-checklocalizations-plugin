using CommonPlayniteShared;
using Playnite.SDK;
using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonPluginsShared.Converters
{
    public class LocalDateYMConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null && (DateTime)value != default(DateTime))
                {
                    string ShortDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    string[] result = Regex.Split(ShortDatePattern, @"[/\.\- ]");
                    string YMDatePattern = string.Empty;
                    foreach(string str in result)
                    {
                        if (!str.Contains("d", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (YMDatePattern.IsNullOrEmpty())
                            {
                                YMDatePattern = str;
                            }
                            else
                            {
                                YMDatePattern += CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator + str;
                            }
                        }
                    }

                    DateTime dt = ((DateTime)value).ToLocalTime();
                    return dt.ToString(YMDatePattern);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
