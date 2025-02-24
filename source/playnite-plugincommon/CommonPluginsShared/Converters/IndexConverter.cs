using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace CommonPluginsShared.Converters
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is ListBoxItem item)
                {
                    ListBox listView = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                    if (listView == null)
                    {
                        return "-1";
                    }
                    int index = listView.ItemContainerGenerator.IndexFromContainer(item);
                    return index.ToString();
                }

                return string.Empty;
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
